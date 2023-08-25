using BrickSchema.Net.Behaviors;
using BrickSchema.Net.Classes;
using BrickSchema.Net.EntityProperties;
using BrickSchema.Net.Shapes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace BrickSchema.Net
{
    public partial class BrickBehavior : BrickEntity
    {
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

        }
        private BrickBehavior(BrickEntity entity) : base(entity) //this is for cloning only
        {

        }

        public BrickBehavior(string behaviorFunction, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            Init(behaviorFunction, behaviorName, weight, logger);
        }

        public BrickBehavior(BehaviorFunction.Types behaviorFunction, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            Init(behaviorFunction.ToString(), behaviorName, weight, logger);
        }

        private void Init(string behaviorFunction, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            AddShape<BehaviorFunction>(behaviorFunction);
            SetProperty(PropertiesEnum.Name, behaviorName);
            SetProperty(PropertiesEnum.Running, false);
            SetProperty(PropertiesEnum.Weight, weight);
            Type = this.GetType().Name;
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

        public BehaviorValue SetConformance(double value)
        {
            SetProperty(PropertiesEnum.Conformance, value);
            BehaviorValue bv = new(PropertiesEnum.Conformance, Id, Type, GetShapeStringValue<BehaviorFunction>());
            bv.SetValue(value);
            return bv;
        }

        public BehaviorValue SetBehaviorValue<T>(string valueName, T value)
        {
            SetProperty(valueName, value);
            BehaviorValue bv = new(valueName, Id, Type, GetShapeStringValue<BehaviorFunction>());
            bv.SetValue(value);
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
                if (!isExecuting && IsRunning && !_executionThread.IsAlive)
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


        // Add a virtual Start method
        protected void RegisterOnTimerTask(int poleRateSeconds)
        {
            if (poleRateSeconds < 10) poleRateSeconds= 10;
            _pollRateSeconds = poleRateSeconds;
            _changedOfValueDelay = -1;
        }

        protected void UnRegisterTime()
        {
            _pollRateSeconds = -1;
        }

        protected void RegisterParentPropertyValueChangedEvent(int delaySeconds = 0)
        {
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
                        List<string> reasons = new();
                        if (!IsBehaviorEnabled())
                        {

                            string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                            text += "\n- This behavior is disabled.";
                            text += "\n- Please enable behavior if needed to run.";

                            Insight = text;
                            Resolution = text;
                        }
                        else if (!IsBehaviorRunnable(reasons))
                        {
                            string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                            text += "\n- This behavior doesn't meet required conditions to run.";
                            text += "\n- Please review technical info for more information.";
                            foreach (var r in reasons)
                            {
                                text += $"\n- {r}";
                            }
                            Insight = text;
                            Resolution = text;
                        }
                        else
                        {

                            var analyticsReturnCode = OnParentPointValueChangedTask(e as Point, out List<BehaviorValue> values);
                            if (analyticsReturnCode == BehaviorTaskReturnCodes.Good)
                            {
                                Parent?.SetBehaviorValue(values);
                            }
                            var insightReturnCode = GenerateInsight(analyticsReturnCode, values, out string insight);
                            string header = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} Analytics Task:  {analyticsReturnCode.ToString()} \n\r\n\r";
                            Insight = $"{header}{insight}";

                            var resolutionReturnCode = GenerateResolution(analyticsReturnCode, values, out string resolution);

                            Resolution = $"{header}{resolution}";
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
                            List<string> reasons = new();
                            if (!IsBehaviorEnabled())
                            {

                                string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                                text += "\n- This behavior is disabled.";
                                text += "\n- Please enable behavior if needed to run.";

                                Insight = text;
                                Resolution = text;
                            }
                            else if (!IsBehaviorRunnable(reasons))
                            {
                                string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                                text += "\n- This behavior doesn't meet required conditions to run.";
                                text += "\n- Please review technical info for more information.";
                                foreach (var r in reasons)
                                {
                                    text += $"\n- {r}";
                                }
                                Insight = text;
                                Resolution = text;
                            }
                            else
                            {
                                var analyticsReturnCode = OnTimerTask(out List<BehaviorValue> values);
                                if (analyticsReturnCode == BehaviorTaskReturnCodes.Good)
                                {
                                    Parent?.SetBehaviorValue(values);
                                }
                                var insightReturnCode = GenerateInsight(analyticsReturnCode, values, out string insight);
                                string header = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} Analytics Task:  {analyticsReturnCode.ToString()} \n\r\n\r";
                                Insight = $"{header}{insight}";

                                var resolutionReturnCode = GenerateResolution(analyticsReturnCode, values, out string resolution);

                                Resolution = $"{header}{resolution}";
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
                        List<string> reasons = new();
                        if (!IsBehaviorEnabled())
                        {

                            string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                            text += "\n- This behavior is disabled.";
                            text += "\n- Please enable behavior if needed to run.";

                            Insight = text;
                            Resolution = text;
                        }
                        else if (!IsBehaviorActive(operatingMode))
                        {
                            SetProperty(PropertiesEnum.BehaviorActive, false);
                            string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                            text += $"\n- This behavior is NOT active based on Operating Mode [{operatingMode?.ToString()}].";

                            Insight = text;
                            Resolution = text;
                        }
                        else if (!IsBehaviorRunnable(reasons))
                        {
                            SetProperty(PropertiesEnum.BehaviorActive, true);
                            string text = $"###### {DateTime.Now.ToLongDateString()} Execution Result {BehaviorTaskReturnCodes.Skip.ToString()} \n\r\n\r";
                            text += "\n- This behavior doesn't meet required conditions to run.";
                            text += "\n- Please review technical info for more information.";
                            foreach (var r in reasons)
                            {
                                text += $"\n- {r}";
                            }
                            Insight = text;
                            Resolution = text;
                        }
                        else
                        {
                            SetProperty(PropertiesEnum.BehaviorActive, true);
                            var analyticsReturnCode = ManualTask(out List<BehaviorValue> values);
                            if (analyticsReturnCode == BehaviorTaskReturnCodes.Good)
                            {
                                Parent?.SetBehaviorValue(values);
                            }
                            var insightReturnCode = GenerateInsight(analyticsReturnCode, values, out string insight);
                            string header = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} Analytics Task:  {analyticsReturnCode.ToString()} \n\r\n\r";
                            Insight = $"{header}{insight}";

                            var resolutionReturnCode = GenerateResolution(analyticsReturnCode, values, out string resolution);

                            Resolution = $"{header}{resolution}";
                        }
                    }


                    catch { }
                   


                }
                catch { }
                isExecuting = false;
                SetProperty(PropertiesEnum.LastExecutionEnd, DateTime.Now);
            }
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


                    returnCode = TechnicalInfo(out List<string> requiredTags, out List<string> optionalTags, out string runCondition);

                    UpdatePointLists(requiredTags, optionalTags);

                    string info = $"## {Name} Technical Information";
                    info += $"Behavior ID: {Id}\r\n";
                    info += $"Behavior Type: {Type}\r\n";
                    if (_pollRateSeconds >= 0)
                    {
                        info += $"Execution Cycle: {_pollRateSeconds} seconds\r\n";
                    }
                    else if (_changedOfValueDelay >= 0)
                    {
                        info += $"Execution Cycle: On Point Value Changed. Delay {_changedOfValueDelay} seconds\r\n";
                    }
                    info += $"Required Point Tags: \r\n";
                    foreach (var tag in requiredTags.Distinct())
                    {
                        info += $"-{tag}\r\n";
                    }

                    info += $"Optional Point Tag: \r\n";
                    foreach (var tag in optionalTags.Distinct())
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
            return null;
        }

        protected List<string> GetRequiredTags()
        {
            List<string> tags = new List<string>();

            foreach (var point in _requiredPoints)
            {
                tags.Add(point.Key);
            }
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
            foreach (var point in _requiredPoints)
            {
                if (point.Value != null) points.Add(point.Value);
            }
            return points;
        }

        protected List<string> GetOptionalTags()
        {
            List<string> tags = new List<string>();
            foreach (var point in _optionalPoints)
            {
                tags.Add(point.Key);
            }
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
            foreach (var point in _optionalPoints)
            {
                if (point.Value != null) points.Add(point.Value);
            }
            return points;
        }

        private void UpdatePointLists(List<string> requireds, List<string> optionals)
        {
            bool gate = true;
            while (gate)
            {
                gate = false;
                foreach (var r in _requiredPoints)
                {
                    if (!requireds.Contains(r.Key))
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
                    if (!optionals.Contains(o.Key))
                    {
                        _optionalPoints.Remove(o.Key);
                        gate = true;
                        break;
                    }
                }
            }
            foreach (var require in requireds.Distinct())
            {
                if (!_requiredPoints.ContainsKey(require)) _requiredPoints.Add(require, null);
                _requiredPoints[require] = Parent.GetPointEntity(require);
            }
            foreach (var optional in optionals.Distinct())
            {
                if (!_optionalPoints.ContainsKey(optional)) _optionalPoints.Add(optional, null);
                _optionalPoints[optional] = Parent.GetPointEntity(optional);
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

        protected virtual bool IsBehaviorRunnable(List<string>? reasons = null)
        {
            bool runnable = true;
            foreach (var p in _requiredPoints)
            {
                if (p.Value == null)
                {
                    runnable = false;
                    reasons?.Add($"Required point tag [{p.Key}] is not mapped.");
                }
            }
            if (runnable)
            {
                reasons?.Add("All required point tags have been mapped.");
            }
            return runnable;
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
        protected virtual BehaviorTaskReturnCodes TechnicalInfo(out List<string> requiredTags, out List<string> optionalTags, out string runCondition)
        {
            requiredTags = new();
            optionalTags = new();
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
            string text = $"";
            if (behaviorValues.Count < 1)
            {
                text += "\n###### Analytics result is empty.";
            }
            else
            {
                text += "\n###### Analytics Results";

                foreach (var bv in behaviorValues)
                {
                    text += $"\n\r**Value Label: {bv.Name}**";
                    if (double.TryParse(bv.Value, out double value))
                    {
                        text += $"\n- Value: {value.ToString("F2")}";
                    }
                    else
                    {
                        text += $"\n- Value: {bv.Value}";
                    }
                    text += $"\n- Timestamp: {bv.Timestamp}";
                    text += $"\n- Weight: {bv.Weight}";
                    text += $"\n- History size: {bv.Histories.Count}";
                    text += "\n\r***";
                }
            }


            insight = text;

            return BehaviorTaskReturnCodes.NotImplemented;
        }

        protected virtual BehaviorTaskReturnCodes GenerateResolution(BehaviorTaskReturnCodes analyticsReturnCode, List<BehaviorValue> behaviorValues, out string resolution)
        {
            resolution = "Not Implimented.";
            return BehaviorTaskReturnCodes.NotImplemented;
        }

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
            var pointOfs = Parent.GetPointOfParent();
            foreach (var point in pointOfs)
            {
                if (point != null)
                {
                    var behaviors = point.Behaviors.Where(x => x.Type?.Equals(type) ?? false);
                    behaviors.Except(args);
                }
            }
            return null;
        }
    }
}
