using BrickSchema.Net.Behaviors;
using BrickSchema.Net.Classes;
using BrickSchema.Net.EntityProperties;
using BrickSchema.Net.Relationships;
using BrickSchema.Net.Shapes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace BrickSchema.Net
{
    public partial class BrickBehavior : BrickEntity
    {

        #region Events
        public event EventHandler<BehaviorExecutedEventArgs> OnBehaviorExecuted;
        #endregion

        #region Private properties
        [JsonIgnore]
        private Thread? _executionThread;
        private int _pollRateSeconds = -1;
        private int _changedOfValueDelay = -1;
        private bool _isOnTimerTaskRunning = false;
        private bool _isOnParentPointValueChangedTaskRunning = false;
        private bool _isInfoRunning = false;
        private DateTime _lastInfoUpdate = DateTime.MinValue;
        private Dictionary<string, Point?> _requiredPoints = new();
        private Dictionary<string, Point?> _optionalPoints = new();
        private Dictionary<string, DateTime> Errors = new Dictionary<string, DateTime>();
        private DateTime LastChangeOfValueRun = DateTime.MinValue;
        #endregion private properties

        #region Protected properties
        [JsonIgnore]
        protected bool isExecuting = false;
        protected ILogger? _logger;
        protected CancellationTokenSource? CancelToken;
        [JsonIgnore]
        protected List<FaultAnalysis> SelfCheckFunctions { get; set; } = new();
        #endregion protected properties

        #region Public properties


        [JsonIgnore]
        public bool IsRunning
        {
            get { return GetProperty<bool>(PropertiesEnum.Running); }
            protected set { SetProperty(PropertiesEnum.Running, value); }
        }


        [JsonIgnore]
        public string Name { get { return GetProperty<string>(PropertiesEnum.Name) ?? string.Empty; } }

        [JsonIgnore]
        public string Description
        {
            get { return GetProperty<string>(PropertiesEnum.Description) ?? string.Empty; }
            protected set { SetProperty(PropertiesEnum.Description, value); }
        }

        [JsonIgnore]
        public string Insight
        {
            get { return GetProperty<string>(PropertiesEnum.Insight) ?? string.Empty; }
            protected set { SetProperty(PropertiesEnum.Insight, value); }
        }

        [JsonIgnore]
        public string Resolution
        {
            get { return GetProperty<string>(PropertiesEnum.Resolution) ?? string.Empty; }
            protected set { SetProperty(PropertiesEnum.Resolution, value); }
        }

        [JsonIgnore]
        public string Info
        {
            get { return GetProperty<string>(PropertiesEnum.Info) ?? string.Empty; }
            protected set { SetProperty(PropertiesEnum.Info, value); }
        }

        [JsonIgnore]
        public DateTime LastExecutionStart
        {
            get
            {
                var d = GetProperty<DateTime?>(PropertiesEnum.LastExecutionStart);
                if (d == null)
                {
                    d = DateTime.Now;
                    SetProperty(PropertiesEnum.LastExecutionStart, d);
                }
                return (DateTime)d;
            }
        }

        [JsonIgnore]
        public DateTime LastExecutionEnd
        {
            get
            {
                var d = GetProperty<DateTime?>(PropertiesEnum.LastExecutionEnd);
                if (d == null)
                {
                    d = DateTime.Now;
                    SetProperty(PropertiesEnum.LastExecutionEnd, d);
                }
                return (DateTime)d;
            }
        }

        [JsonIgnore]
        public double Weight
        {
            get
            {
                var w = GetProperty<double>(PropertiesEnum.Weight);
                return w == 0 ? 1 : w;
            }
        }

        [JsonIgnore]
        public BrickEntity? Parent = null;
        #endregion public properties

        protected bool NotifyError(string error)
        {
            bool answer = false;

            if (Errors.ContainsKey(error))
            {
                var ts = DateTime.Now - Errors[error];
                if (ts.TotalHours >= 24)
                {
                    answer = true;
                    Errors[error] = DateTime.Now;
                }
            }
            else
            {
                answer = true;
                Errors.Add(error, DateTime.Now);
            }

            SetProperty(PropertiesEnum.Errors, Errors);
            SetProperty(PropertiesEnum.HasError, true);
            return answer;
        }


        protected void ClearErrors()
        {
            Errors.Clear();
            SetProperty(PropertiesEnum.HasError, false);
        }

        public BrickBehavior()
        {
            OnBehaviorExecuted = delegate { };  // Initialize with an empty delegate
        }
    
        private BrickBehavior(BrickEntity entity) : base(entity) //this is for cloning only
        {
            OnBehaviorExecuted = delegate { };  // Initialize with an empty delegate
        }

        public BrickBehavior(string behaviorFunction, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            OnBehaviorExecuted = delegate { };  // Initialize with an empty delegate
            Init(behaviorFunction, behaviorName, weight, logger);
        }

        public BrickBehavior(BehaviorFunction.Types behaviorFunction, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            OnBehaviorExecuted = delegate { };  // Initialize with an empty delegate
            Init(behaviorFunction.ToString(), behaviorName, weight, logger);
        }

        private void Init(string behaviorFunction, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            AddShape<BehaviorFunction>(behaviorFunction);
            SetProperty(PropertiesEnum.Name, behaviorName);
            SetProperty(PropertiesEnum.Running, false);
            SetProperty(PropertiesEnum.Weight, weight);
            EntityTypeName = this.GetType().Name;
            SetProperty(PropertiesEnum.BehaviorFunction, behaviorFunction);
            _logger = logger;
            isExecuting = false;
            _executionThread = new Thread(ExecuteTimerTask);
            CancelToken = new();
        }
        

        #region Logger
        public void SetLogger(ILogger? logger)
        {
            _logger = logger;
        }

        public bool IsLogger
        {
            get { return _logger != null; }
        }
        #endregion Logger


        #region public functions
        public override BrickBehavior Clone()
        {
            var clone = new BrickBehavior(base.Clone());
            //clone.Parent = Parent?.Clone();
            return clone;
        }

        

        public BehaviorValue SetBehaviorValue<T>(string valueName, T value)
        {
            SetProperty(valueName, value);
            BehaviorValue bv = new(valueName, Id, EntityTypeName, GetShapeStringValue<BehaviorFunction>());
            bv.SetValue(value);
            if (valueName.Equals("Faulted") && bv.FaultType == BehaviorFaultTypes.None)
            {
                bv.FaultType = BehaviorFaultTypes.Fault;
            }
            return bv;
        }

        public BehaviorValue SetBehaviorValue<T>(PropertiesEnum valueName, T value, string description = "")
        {
            return SetBehaviorValue(valueName.ToString(), value);
        }

        #endregion public fucntions

        #region CallBack
        public async void OnTimerTick()
        {
            GenerateInfo();

            if (_changedOfValueDelay >= 0)
            {
                if (LastChangeOfValueRun.AddMinutes(15) < DateTime.Now) {
                    LastChangeOfValueRun = DateTime.Now;
                    await ProcessParentPointValueChange(new(), "", true); 
                }
            } 
            if (IsTimeToRun())
            {
                if (!isExecuting && IsRunning && !(_executionThread?.IsAlive??false))
                {

                    try
                    {
                        CancelToken = new CancellationTokenSource();
                        _executionThread = new Thread(ExecuteTimerTask);
                        _executionThread.Start();
                        _executionThread.Join();

                    }
                    catch (Exception ex)
                    {

                        _logger?.LogError(ex, $"Bahavior OnTimerTick: Excpetion: {ex.Message}");
                    }

                }
            }

        }

        public void Start()
        {

            IsRunning = true;
            Load();
            // Default implementation does nothing
        }

        // Add a virtual Stop method
        public void Stop()
        {
            IsRunning = false;
            Unload();
            // Default implementation does nothing
        }
        #endregion callback

        #region default Functions

        internal override async void HandleOnPropertyValueChanged(string sourceId, string propertyName)
        {
            var e = Parent?.OtherEntities.FirstOrDefault(x => x.Id == sourceId);
            if (e == null) return;

            await ProcessParentPointValueChange(e, propertyName);
        }

        protected void RegisterManualTask(List<string> requiredTags, List<string> optionalTags)
        {
            UpdatePointLists(requiredTags, optionalTags);
            _changedOfValueDelay = -1;
            _pollRateSeconds -= 1;
        }

        // Add a virtual Start method
        protected void RegisterOnTimerTask(int poleRateSeconds, List<string> requiredTags, List<string> optionalTags)
        {
            UpdatePointLists(requiredTags, optionalTags);
            if (poleRateSeconds < 10) poleRateSeconds= 10;
            _pollRateSeconds = poleRateSeconds;
            _changedOfValueDelay = -1;

        }

        protected void UnRegisterTime()
        {
            _pollRateSeconds = -1;
        }

        protected void RegisterParentPropertyValueChangedEvent(List<string> requiredTags, List<string> optionalTags, int delaySeconds = 0)
        {
            UpdatePointLists(requiredTags, optionalTags);
            if (delaySeconds < 0) delaySeconds = 0;
            _changedOfValueDelay = delaySeconds;
            _pollRateSeconds -= 1;
        }

        protected void UnRegisterParentPropertyValueChangedEvent()
        {
            _changedOfValueDelay = -1;
        }
        private bool IsTimeToRun()
        {
            if (_pollRateSeconds < 0) return false;
            DateTime lastExecutionTime = GetProperty<DateTime>(PropertiesEnum.LastExecutionStart);
            bool ok = lastExecutionTime.AddSeconds(_pollRateSeconds) <= DateTime.Now;
            return ok;
        }


        private async Task ProcessParentPointValueChange(BrickEntity e, string propertyName, bool periodicRun = false)
        {
            
            if ((!(e is Point) || !propertyName.Equals(PropertiesEnum.Value)) && !periodicRun) return;
            
            if (!_isOnParentPointValueChangedTaskRunning)
            {
                _isOnParentPointValueChangedTaskRunning = true;
                SetProperty(PropertiesEnum.LastExecutionStart, DateTime.Now);
                try
                {

                    if (_changedOfValueDelay >= 0)
                    {
                        await Task.Delay(1000 * _changedOfValueDelay);
                        if (PreTaskRunCheck())
                        {

                            
                            try
                            {
                                var returnCode = OnParentPointValueChangedTask(e as Point, out List<BehaviorValue> values);
                                ProcessTaskCompleted(returnCode, values);
                            }
                            catch (Exception ex)
                            {
                                ProcessTaskException(MethodBase.GetCurrentMethod()?.Name ?? "ExecuteBehavior", ex);
                            }



                        }
                    }
                }
                catch { }
                _isOnParentPointValueChangedTaskRunning = false;
                SetProperty(PropertiesEnum.LastExecutionEnd, DateTime.Now);
            }
        }


        public void ExecuteTimerTask()
        {
            if (!isExecuting)
            {
                isExecuting = true;
                try
                {
                    SetProperty(PropertiesEnum.LastExecutionStart, DateTime.Now);

                    if (!_isOnTimerTaskRunning)
                    {
                        _isOnTimerTaskRunning = true;
                        try
                        {
                            if (PreTaskRunCheck()) 
                            {
                                try
                                {
                                    var returnCode = OnTimerTask(out List<BehaviorValue> values);
                                    ProcessTaskCompleted(returnCode, values);
                                }
                                catch (Exception ex)
                                {
                                    ProcessTaskException(MethodBase.GetCurrentMethod()?.Name ?? "ExecuteBehavior", ex);
                                }
                            }

                            
                        }


                        catch { }
                        _isOnTimerTaskRunning = false;
                    }

                }
                catch { }
                isExecuting = false;
                SetProperty(PropertiesEnum.LastExecutionEnd, DateTime.Now);
            }
        }

        public void ExecuteManualTask<T>(T? operatingMode = default(T?))
        {
            if (!isExecuting)
            {
                isExecuting = true;
                try
                {
                    SetProperty(PropertiesEnum.LastExecutionStart, DateTime.Now);

                    
                    try
                    {
                       
                        
                        if (PreTaskRunCheck(operatingMode))
                        {
                            SetProperty(PropertiesEnum.BehaviorActive, true);

                            try
                            {
                                var returnCode = ManualTask(out List<BehaviorValue> values);
                                ProcessTaskCompleted(returnCode, values);


                            } catch (Exception ex)
                            {
                                ProcessTaskException(MethodBase.GetCurrentMethod()?.Name??"ExecuteBehavior", ex);
                            }
                            
                        }
                    }
                    catch { }
                }
                catch { }
                isExecuting = false;
                SetProperty(PropertiesEnum.LastExecutionEnd, DateTime.Now);
            }
        }

        private bool PreTaskRunCheck<T>(T? operatingMode = default(T?))
        {
            // Enable - allow to run
            // Runnable - can it run
            // Active - should it run

            if (Parent.GetProperty<string>(PropertyName.Name).Equals("SIM_FCU_1") && Name.Equals("FCU Behavior"))
            {
                bool debug = true;
            }


            if (!IsBehaviorEnabled()) // Allow it to run
            {

                string text = $"***{DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()}*** \n\r\n\r";
                text += "\n- This behavior is disabled.";
                text += "\n- Please enable behavior if needed to run.";

                Insight = text;
                Resolution = "No action required";
                return false;
            }


            Dictionary<string, List<string>> reasons = IsBehaviorRunnable(); // Can it run?
            SetProperty(PropertiesEnum.BehaviorRunnable, !reasons.Any());
            if (reasons.Any())
            {
                SetProperty(PropertiesEnum.BehaviorActive, true);
                string text = $"***{DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()}*** \n\r\n\r";
                text += "\n- This behavior doesn't meet required conditions to run.";
                text += "\n- Please review technical info for more information.";

                foreach (var r in reasons)
                {

                    text += $"\n- {r.Key}";
                    int count = 1;
                    foreach (var s in r.Value)
                    {
                        text += $"\n     {count}. {s}";
                    }

                }
                Insight = text;
                Resolution = "This behavior doesn't meet required conditions to run. Please map required point tags.";
                return false;
            }



            if (operatingMode == null) // Should it run?
            {
                SetProperty(PropertiesEnum.BehaviorActive, true);
            }
            else if (operatingMode != null)
            {
                bool IsActive = IsBehaviorActive(operatingMode);
                SetProperty(PropertiesEnum.Mode, operatingMode);
                if (!IsActive)
                {
                    SetProperty(PropertiesEnum.BehaviorActive, false);
                    string text = $"***{DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()}*** \n\r\n\r";
                    text += $"\n- This behavior is NOT applicable to current Operating Mode [***{operatingMode?.ToString()}***].";

                    Insight = text;
                    Resolution = "No action required";
                    return false;
                }
                
            }
             

            return true;
        }

        private bool PreTaskRunCheck()
        {
            string? n = null;
            return PreTaskRunCheck(n);
        }

        private void ProcessTaskCompleted(BehaviorTaskReturnCodes taskReturnCode, List<BehaviorValue> values)
        {
            if (taskReturnCode == BehaviorTaskReturnCodes.Good || taskReturnCode == BehaviorTaskReturnCodes.HasWarning)
            {
                

                var thReturnCode = FaultWorkflow1_Threshold(taskReturnCode, values);
                if (thReturnCode == BehaviorTaskReturnCodes.Good || thReturnCode == BehaviorTaskReturnCodes.HasWarning)
                {
                    
                    List<BehaviorValue> selftestBV = new List<BehaviorValue>();
                    var stReturnCode = FaultWorkflow2_SelfTest(thReturnCode, values, out selftestBV);
                    if (stReturnCode == BehaviorTaskReturnCodes.Good || stReturnCode == BehaviorTaskReturnCodes.HasWarning)
                    {
                        foreach (var fault in selftestBV)
                        {
                            fault.FaultType = BehaviorFaultTypes.FaultAnalysis;
                            values.Add(fault);
                        }

                        List<BehaviorValue> faultBV = new List<BehaviorValue>();
                        var fReturnCode = FaultWorkflow3_GenerateFault(stReturnCode, values, out faultBV);
                        if (fReturnCode == BehaviorTaskReturnCodes.Good || fReturnCode == BehaviorTaskReturnCodes.HasWarning)
                        {
                            foreach (var fault in faultBV)
                            {
                                fault.FaultType = BehaviorFaultTypes.Fault;
                                values.Add(fault);
                            }
                        }

                    }
                }
                Parent?.SetBehaviorValue(values);
            }
            OnBehaviorExecuted?.Invoke(null, new() { ParentId = Parent?.Id ?? "0", Values = values, TaskReturnCode = taskReturnCode });
            
            UpdateInsightAndResolution(taskReturnCode, values);
        }

        private void ProcessTaskException(string methodName, Exception ex)
        {
            string header = $"*** {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")} {DateTime.Now.ToShortTimeString()} Analytics Task:  {BehaviorTaskReturnCodes.HasException.ToString()}*** \n\r\n\r";
            Insight = $"{header}{ex.Message} \n\r {ex.ToString()}";
            Resolution = $"{header}Please see Insight for detail.";
            _logger?.LogCritical(ex, $"Exception Method [{methodName}] Behavior [{Name}] Parent [{Parent?.GetProperty<string>(PropertiesEnum.Name)}]");
        }

        private void UpdateInsightAndResolution(BehaviorTaskReturnCodes analyticsReturnCode, List<BehaviorValue> values)
        {
            
            var insightReturnCode = GenerateInsight(analyticsReturnCode, values, out string insight);
            //string header = $"###### {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} Analytics Task:  {analyticsReturnCode.ToString()} \n\r\n\r";
            Insight = insight;

            var resolutionReturnCode = GenerateResolution(analyticsReturnCode, values, out string resolution);

            Resolution = resolution;
        }

        
        private void GenerateInfo()
        {
            if (!_isInfoRunning && _lastInfoUpdate.AddMinutes(10) <= DateTime.Now)
            {
                _isInfoRunning = true;
                try
                {
                    var returnCode = GenerateDescription(out string description);
                    Description = description;


                    returnCode = TechnicalInfo(out string runCondition);


                    string info = $"## {Name} Technical Information";
                    info += $"Behavior ID: {Id}\r\n";
                    info += $"Behavior Type: {EntityTypeName}\r\n";
                    if (_pollRateSeconds >= 0)
                    {
                        info += $"Execution Cycle: {_pollRateSeconds} seconds\r\n";
                    }
                    else if (_changedOfValueDelay >= 0)
                    {
                        info += $"Execution Cycle: On Point Value Changed. Delay {_changedOfValueDelay} seconds\r\n";
                    }
                    info += $"Required Point Tags: \r\n";
                    foreach (var tag in _requiredPoints.Distinct())
                    {
                        info += $"-{tag}\r\n";
                    }

                    info += $"Optional Point Tag: \r\n";
                    foreach (var tag in _optionalPoints.Distinct())
                    {
                        info += $"-{tag}\r\n";
                    }

                    info += "\r\n";

                    info += $"Run Condition: {runCondition}";
                    info += "\r\n";

                    info += "### Mapped Entities:\r\n";
                    if (Parent != null)
                    {
                        info += $"Parent: {Parent.GetProperty<string>(PropertiesEnum.Name)}\r\n";
                        info += $"Required Point Tag: \r\n";
                        foreach (var point in _requiredPoints)
                        {

                            info += $"- [{point.Value != null}] {point.Key}\r\n";
                        }
                        info += $"Optional Point Tag: \r\n";
                        foreach (var point in _optionalPoints)
                        {
                            info += $"- [{point.Value != null}] {point.Key}\r\n";
                        }

                    }
                    else
                    {
                        info += "Error: Missing Parent Entity.\r\n";
                    }

                    info += $"\r\n";

                    Info = info;


                }
                catch { }
                _lastInfoUpdate = DateTime.Now;
                _isInfoRunning = false;
            }
        }

        protected Point? GetPoint(string Tag)
        {
            if (_requiredPoints.ContainsKey(Tag)) return _requiredPoints[Tag];
            if (_optionalPoints.ContainsKey(Tag)) return _optionalPoints[Tag];
            return Parent?.GetPointEntity(Tag);

        }

        protected List<string> GetRequiredTags()
        {
            
            List<string> tags = GetProperty<List<string>>(PropertyName.RequiredTags)??new();

            return tags;
        }

        protected Point? GetTaggedPoint(string Tag)
        {
            if (_requiredPoints.ContainsKey(Tag)) return _requiredPoints[Tag];

            return null;
        }
        protected List<string> GetRequiredMappedPointTags()
        {
            var points = GetRequiredPoints();
            List<string> tags = new();
            foreach (var point in points)
            {
                List<Tag> foundTags = point.GetTags();
                foreach (var t in foundTags)
                {
                    if (!tags.Contains(t.Name)) tags.Add(t.Name);
                }
                
            }
            return tags;
        }

        protected List<Point> GetRequiredPoints()
        {
            List<Point> points = new List<Point>();
            foreach (var point in _requiredPoints.ToArray())
            {
                if (point.Value != null) points.Add(point.Value);
                else
                {
                    var p = Parent?.GetPointEntity(point.Key);
                    if (p != null)
                    {
                        points.Add(p);
                        _requiredPoints[point.Key] = p;
                    }
                }
            }
            return points;
        }

        protected List<string> GetOptionalTags()
        {
            List<string> tags = GetProperty<List<string>>(PropertyName.OptionalTags) ?? new();

            return tags;
        }
        protected List<string> GetOptionalMappedPointTags()
        {
            var points = GetOptionalPoints();
            List<string> tags = new();
            foreach (var point in points)
            {
                List<Tag> foundTags = point.GetTags();
                foreach (var t in foundTags)
                {
                    if (!tags.Contains(t.Name)) tags.Add(t.Name);
                }

            }
            return tags;
        }

        protected Point? GetOptionalPoint(string Tag)
        {
            if (_optionalPoints.ContainsKey(Tag)) return _optionalPoints[Tag];

            return null;
        }

        protected List<Point> GetOptionalPoints()
        {
            List<Point> points = new List<Point>();
            var optionalTags = GetProperty<List<string>>(PropertyName.OptionalTags);
            foreach (var tag in optionalTags ?? new())
            {

                var p = Parent?.GetPointEntity(tag);
                if (p != null)
                {
                    points.Add(p);

                }

            }
            return points;
        }

        private void UpdatePointLists(List<string> requiredTags, List<string> optionalTags)
        {
           
            SetProperty(PropertyName.RequiredTags, requiredTags);
            SetProperty(PropertyName.OptionalTags, optionalTags);

            bool gate = true;
            while (gate)
            {
                gate = false;
                foreach (var r in _requiredPoints)
                {
                    if (!requiredTags.Contains(r.Key))
                    {
                        _requiredPoints.Remove(r.Key);
                        gate = true;
                        break;
                    }
                }
            }
            gate = true;
            while (gate)
            {
                gate = false;
                foreach (var o in _optionalPoints)
                {
                    if (!optionalTags.Contains(o.Key))
                    {
                        _optionalPoints.Remove(o.Key);
                        gate = true;
                        break;
                    }
                }
            }
            foreach (var require in requiredTags.Distinct())
            {
                if (!_requiredPoints.ContainsKey(require)) _requiredPoints.Add(require, null);
                _requiredPoints[require] = Parent?.GetPointEntity(require);
            }
            foreach (var optional in optionalTags.Distinct())
            {
                if (!_optionalPoints.ContainsKey(optional)) _optionalPoints.Add(optional, null);
                _optionalPoints[optional] = Parent?.GetPointEntity(optional);
            }
        }

        protected bool IsBehaviorEnabled()
        {
            if (IsProperty(PropertiesEnum.BehaviorEnable)) return GetProperty<bool>(PropertiesEnum.BehaviorEnable);
            return true;
        }

        protected virtual bool IsBehaviorActive<T>(T? operatingMode = default(T?))
        {
            if (operatingMode == null) return true;
            return false;
        }

        protected virtual Dictionary<string, List<string>> IsBehaviorRunnable()
        {
            var reasons = IsRequiredTagsMapped();
            return reasons;
        }

        protected Dictionary<string, List<string>> IsRequiredTagsMapped(params (string Name, List<string> Tags)[]tags)
        {
            Dictionary<string, List<string>> reasons = new();
            if (Parent == null)
            {
                reasons.Add("Unknown parent.", new());
                return reasons;
            }
            var MapRequired = GetRequiredMappedPointTags();
            var MapOptional = GetOptionalMappedPointTags();

            var Required = GetRequiredTags();

            var MissingRequired = Required.Except(MapRequired);

            if (MissingRequired.Count() > 0)
            {
                reasons.Add($"Missing required point tags:", MissingRequired.ToList());
            }

            foreach (var tagList in tags)
            {
                var hasTags = tagList.Tags.Any(x => MapOptional.Contains(x) || MapRequired.Contains(x));
                if (!hasTags)
                {
                    reasons.Add($"Required one of the following {tagList.Name} tags:", tagList.Tags);
                }
            }

            return reasons;
        }

        protected Dictionary<string, List<string>> IsRunTagsActive(params string[] tags)
        {
            Dictionary<string, List<string>> reasons = new();
            if (Parent == null)
            {
                reasons.Add("Unknown parent.", new());
                return reasons;
            }
            Point? Enable = GetSpecifiedPoint(tags.ToList());
            string response = Enable != null ? "exists" : "does not exist";
            if (Enable != null)
            {
                response = response + $" and it's point name is '{Enable.GetProperty<string>(PropertiesEnum.Name)}'";
            }
            _logger?.LogInformation($"{Parent.GetProperty<string>(PropertiesEnum.Name)} Run Status point {response}");
            _logger?.LogInformation($"{Parent.GetProperty<string>(PropertiesEnum.Name)} Run Status point value is {(Enable?.Value > 0)}");

            if ((Enable?.Value.HasValue ?? false))
            {
                if (Enable?.Value == 0)
                {
                    reasons.Add("Equipment is not operating to run process analytic", new());
                }

            }
            else
            {
                reasons.Add("Unable to determine operating state to process analytic", new());
            }

            return reasons;
        }

        protected Point? GetSpecifiedPoint(List<string> tags)
        {

            if (tags == null)
                return null;

            foreach (var tag in tags)
            {
                var Point = Parent?.GetPointEntity(tag);
                if (Point != null)
                {
                    return Point;
                }
            }
            return null;
        }

        protected virtual BehaviorTaskReturnCodes ManualTask(out List<BehaviorValue> behaviorValues)
        {
            behaviorValues = new();
            return BehaviorTaskReturnCodes.NotImplemented;
        }
        protected virtual BehaviorTaskReturnCodes OnTimerTask(out List<BehaviorValue> behaviorValues)
        {
            behaviorValues = new();
            return BehaviorTaskReturnCodes.NotImplemented;
        }
        protected virtual BehaviorTaskReturnCodes OnParentPointValueChangedTask(Point? point, out List<BehaviorValue> behaviorValues)
        {
            behaviorValues = new();
            return BehaviorTaskReturnCodes.NotImplemented;
        }
        protected virtual BehaviorTaskReturnCodes TechnicalInfo(out string runCondition)
        {
            runCondition = "Not Implimented";
            return BehaviorTaskReturnCodes.NotImplemented;
        }
        protected virtual BehaviorTaskReturnCodes GenerateDescription(out string description)
        {
            description = "Not Implimented.";
            return BehaviorTaskReturnCodes.NotImplemented;
        }
        protected virtual BehaviorTaskReturnCodes GenerateInsight(BehaviorTaskReturnCodes analyticsReturnCode, List<BehaviorValue> behaviorValues, out string insight)
        {
            var insightBuilder = new StringBuilder();
            insightBuilder.AppendLine($"***{DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")} Execution Result {analyticsReturnCode.ToString()}*** \n\r\n\r");
            switch (analyticsReturnCode)
            {
                case BehaviorTaskReturnCodes.Good:
                    insightBuilder.AppendLine("Analysis Complete.");
                    break;
                case BehaviorTaskReturnCodes.HasException:
                    insightBuilder.AppendLine("An exception occurred during analysis.");
                    break;
                case BehaviorTaskReturnCodes.Skip:
                    insightBuilder.AppendLine("Analysis was skipped due to run status or other conditions.");
                    break;
                default:
                    insightBuilder.AppendLine("Analysis completed with unexpected return code.");
                    break;
            }

            if (behaviorValues != null && behaviorValues.Any())
            {
                // Example of processing behaviorValues to generate insights

                insightBuilder.AppendLine("Data available but insight is not implemented.");

                // Add more insights as required based on the behaviorValues
            }
            else
            {
                insightBuilder.AppendLine("No data available to generate insights.");
            }

            insight = insightBuilder.ToString();
            return analyticsReturnCode;
        }

        protected virtual BehaviorTaskReturnCodes GenerateResolution(BehaviorTaskReturnCodes analyticsReturnCode, List<BehaviorValue> behaviorValues, out string resolution)
        {
            resolution = "Not Implimented.";
            return BehaviorTaskReturnCodes.NotImplemented;
        }

        protected virtual BehaviorTaskReturnCodes FaultWorkflow1_Threshold(BehaviorTaskReturnCodes analyticsReturnCode, List<BehaviorValue> analyticsBehaviorValues)
        {
          
            return BehaviorTaskReturnCodes.NotImplemented;
        }

        protected virtual BehaviorTaskReturnCodes FaultWorkflow2_SelfTest(BehaviorTaskReturnCodes threadholdReturnCode, List<BehaviorValue> analyticsBehaviorValues, out List<BehaviorValue> faultAnalysisValues)
        {
            faultAnalysisValues = new();
            if (SelfCheckFunctions.Count > 0)
            {
                
                foreach (var a in SelfCheckFunctions)
                {
                    var result = a.RunActivity(this.Parent);
                    var b = new BehaviorValue(a.ActivityName, this);
                    b.SetValue(result);
                    faultAnalysisValues.Add(b);
                }
                return BehaviorTaskReturnCodes.Good;
            }
            return BehaviorTaskReturnCodes.NotImplemented;
        }

        protected virtual BehaviorTaskReturnCodes FaultWorkflow3_GenerateFault(BehaviorTaskReturnCodes selfTestReturnCode, List<BehaviorValue> analyticsBehaviorValues, out List<BehaviorValue> faultValues)
        {
            faultValues = new();
            return BehaviorTaskReturnCodes.NotImplemented;
        }

        //protected virtual BehaviorTaskReturnCodes FaultWorkflow3A_GenerateAlarm(BehaviorTaskReturnCodes selfTestReturnCode, List<BehaviorValue> analyticsBehaviorValues, out List<BehaviorValue> alarmValues)
        //{
        //    alarmValues = new();
        //    return BehaviorTaskReturnCodes.NotImplemented;
        //}
        protected virtual void Load() { }

        protected virtual void Unload() { }

        #endregion default Functions




        public BrickEntity? AskAssociatedWith(params dynamic[] args)
        {

            if (Parent == null) { return null; }

            return null;
        }

        public BrickEntity? AskFedby(params dynamic[] args)
        {
            if (Parent == null) { return null; }

            return null;
        }

        public BrickEntity? AskMeterBy(params dynamic[] args)
        {
            if (Parent == null) { return null; }

            return null;
        }

        public BrickEntity? AskPartOf(params dynamic[] args)
        {
            if (Parent == null) { return null; }

            return null;
        }

        public BrickEntity? AskPointOfParent(string type, params dynamic[] args)
        {
            if (Parent == null) { return null; }
            var pointOfs = Parent.GetChildEntities<PointOf, Point>();
            foreach (var point in pointOfs)
            {
                if (point != null)
                {
                    var behaviors = point.Behaviors.Where(x => x.EntityTypeName?.Equals(type) ?? false);
                    behaviors.Except(args);
                }
            }
            return null;
        }
    }
}
