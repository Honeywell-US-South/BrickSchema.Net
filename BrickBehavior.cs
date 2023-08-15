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
        private bool _executeByTimer = true;
        private bool _isTaskRunning = false;
        private bool _isOnTimerTaskRunning = false;
        private bool _isOnParentPointValueChangedTaskRunning = false;
        private bool _isInfoRunning = false;
        private bool _isDescriptionRunning = false;
        private bool _isInsightRunning = false;
        private bool _isResolutionRunning = false;
        private DateTime _lastInfoUpdate = DateTime.MinValue;
        private Dictionary<string, DateTime> Errors = new Dictionary<string, DateTime>();
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
            protected set { AddOrUpdateProperty(PropertiesEnum.Running, value); }
        }

        [JsonIgnore]
        public string BehaviorMode { 
            get { return GetProperty<string>(PropertiesEnum.BehaviorMode) ?? string.Empty; }
        }

        [JsonIgnore]
        public string Name { get { return GetProperty<string>(PropertiesEnum.Name)??string.Empty; } }

        [JsonIgnore]
        public string Description { 
            get { return GetProperty<string>(PropertiesEnum.Description) ?? string.Empty; } 
            protected set { AddOrUpdateProperty(PropertiesEnum.Description, value); }
        }

        [JsonIgnore]
        public string Insight { 
            get { return GetProperty<string>(PropertiesEnum.Insight) ?? string.Empty; }
            protected set { AddOrUpdateProperty(PropertiesEnum.Insight, value); }
        }

        [JsonIgnore]
        public string Resolution { 
            get { return GetProperty<string>(PropertiesEnum.Resolution) ?? string.Empty; }
            protected set { AddOrUpdateProperty(PropertiesEnum.Resolution, value); }
        }

        [JsonIgnore]
        public string Info
        {
            get { return GetProperty<string>(PropertiesEnum.Info) ?? string.Empty; }
            protected set { AddOrUpdateProperty(PropertiesEnum.Info, value); }
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
                    AddOrUpdateProperty(PropertiesEnum.LastExecutionStart, d);
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
                    AddOrUpdateProperty(PropertiesEnum.LastExecutionEnd, d);
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
                return w == 0 ? 1: w;
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
            } else
            {
                answer = true;
                Errors.Add(error, DateTime.Now);
            }

            AddOrUpdateProperty(PropertiesEnum.Errors, Errors);
            AddOrUpdateProperty(PropertiesEnum.HasError, true);
            return answer;
        }


        protected void ClearErrors()
        {
            Errors.Clear();
            AddOrUpdateProperty(PropertiesEnum.HasError, false);
        }

        public BrickBehavior()
        {

        }
        private BrickBehavior(BrickEntity entity):base(entity) //this is for cloning only
        {

        } 

        public BrickBehavior(string behaviorType, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            Init(behaviorType, behaviorName, weight, logger);
        }

        public BrickBehavior(BehaviorFunction.Types behaviorType, string behaviorName, double weight = 1, ILogger? logger = null)
        {
           Init(behaviorType.ToString(), behaviorName, weight, logger);
        }

        private void Init(string behaviorType, string behaviorName, double weight = 1, ILogger? logger = null)
        {
            AddShape<BehaviorFunction>(behaviorType.ToString());
            AddOrUpdateProperty(PropertiesEnum.Name, behaviorName);
            AddOrUpdateProperty(PropertiesEnum.Running, false);
            AddOrUpdateProperty(PropertiesEnum.Weight, weight);
            Type = this.GetType().Name;
            AddOrUpdateProperty(PropertiesEnum.BehaviorType, behaviorType);
            _logger = logger;
            isExecuting = false;
            _executionThread = new Thread(Execute);
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
            AddOrUpdateProperty(PropertiesEnum.Conformance, value);
            BehaviorValue bv = new(PropertiesEnum.Conformance, Id, Name, Type, BehaviorMode);
            bv.SetValue(value);
            return bv;

        }

        public BehaviorValue SetBehaviorValue<T>(string valueName, T value, string description = "")
        {
            AddOrUpdateProperty(valueName, value);
            BehaviorValue bv = new(valueName, Id, Name, Type, BehaviorMode);
            bv.SetValue(value);
            bv.Description = description;
            return bv;
        }

        public BehaviorValue SetBehaviorValue<T>(PropertiesEnum valueName, T value, string description = "")
        {
            return SetBehaviorValue(valueName.ToString(), value, description);
        }
        
        #endregion public fucntions

        #region CallBack
        public void OnTimerTick()
        {
            if (_executeByTimer)
            {
                if (!isExecuting && IsRunning && !_executionThread.IsAlive)
                {
                    
                    try
                    {
                        CancelToken = new CancellationTokenSource();
                        _executionThread = new Thread(Execute);
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

        private async Task ProcessParentPointValueChange(BrickEntity e, string propertyName)
        {
            if (!(e is Point) || !propertyName.Equals(PropertiesEnum.Value)) return;
            if (!_isOnParentPointValueChangedTaskRunning)
            {
                _isOnParentPointValueChangedTaskRunning = true;
                try
                {
                    if (_changedOfValueDelay >= 0)
                    {
                        await Task.Delay(1000 * _changedOfValueDelay);

                        var returnCode = OnParentPointValueChangedTask(e as Point, out List<BehaviorValue> values);
                        if (returnCode == BehaviorTaskReturnCodes.Good)
                        {
                            Parent?.SetBehaviorValue(values);
                        }

                    }
                }
                catch { }
                _isOnParentPointValueChangedTaskRunning = false;
            }
        }
        // Add a virtual Start method
        protected void RegisterOnTimerTask(int poleRateSeconds)
        {
            _pollRateSeconds = poleRateSeconds;
            _changedOfValueDelay = -1;
        }
        
        protected void UnRegisterTime()
        {
            _pollRateSeconds = -1;
        }

        protected void RegisterParentPropertyValueChangedEvent(int delaySeconds = 0)
        {
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
            bool ok = lastExecutionTime.AddSeconds(_pollRateSeconds) > DateTime.Now;
            return ok;
        }
        
        public void Execute()
        {
            if (!isExecuting)
            {
                isExecuting = true;
                try
                {
                    AddOrUpdateProperty(PropertiesEnum.LastExecutionStart, DateTime.Now);
                    // Default implementation does nothing
                    if (!_isTaskRunning)
                    {
                        _isTaskRunning = true;
                        try
                        {
                            var returnCode = ProcessTask(out List<BehaviorValue> values);
                            if (returnCode == BehaviorTaskReturnCodes.Good)
                            {
                                Parent?.SetBehaviorValue(values);
                            }
                        }
                        catch { }
                        _isTaskRunning = false;
                    }
                    if (!_isOnTimerTaskRunning)
                    {
                        _isOnTimerTaskRunning = true;
                        try
                        {
                            if (IsTimeToRun())
                            {
                                var returnCode = OnTimerTask(out List<BehaviorValue> values);
                                if (returnCode == BehaviorTaskReturnCodes.Good)
                                {
                                    Parent?.SetBehaviorValue(values);
                                }
                            }
                        }
                        catch { }
                        _isOnTimerTaskRunning = false;
                    }

                    GenerateInfo();

                    if (!_isDescriptionRunning)
                    {
                        _isDescriptionRunning = true;
                        try
                        {
                            var returnCode = GenerateDescription(out string description);
                            Description = string.Empty;
                            if (returnCode != BehaviorTaskReturnCodes.Good)
                            {
                                Description = $"**** {DateTime.Now.ToLongDateString()} Execution Result {returnCode.ToString()} **** \n\r\n\r";
                            }
                            Description += description;
                        }
                        catch { }
                        _isDescriptionRunning = false;
                    }
                    if (!_isInsightRunning)
                    {
                        _isInsightRunning = true;
                        try
                        {
                            var returnCode = GenerateInsight(out string insight);
                            Insight = string.Empty;
                            if (returnCode != BehaviorTaskReturnCodes.Good)
                            {
                                Insight = $"**** {DateTime.Now.ToLongDateString()} Execution Result {returnCode.ToString()} **** \n\r\n\r";
                            }
                            Insight += insight;
                        }
                        catch { }
                        _isInsightRunning = false;
                    }
                    if (!_isResolutionRunning)
                    {
                        _isResolutionRunning = true;
                        try
                        {
                            var returnCode = GenerateResolution(out string resolution);
                            Resolution = string.Empty;
                            if (returnCode != BehaviorTaskReturnCodes.Good)
                            {
                                Resolution = $"**** {DateTime.Now.ToLongDateString()} Execution Result {returnCode.ToString()} **** \n\r\n\r";
                            }
                            Resolution += resolution;
                        }
                        catch { };
                        _isResolutionRunning = false;
                    }
                    
                }
                catch { }
                isExecuting = false;
                AddOrUpdateProperty(PropertiesEnum.LastExecutionEnd, DateTime.Now);
            }
        }

        private void GenerateInfo()
        {
            if (!_isInfoRunning && _lastInfoUpdate.AddMinutes(10) <= DateTime.Now)
            {
                _isInfoRunning = true;
                try
                {
                    var returnCode = TechnicalInfo(out List<string> requiredTags, out List<string> optionalTags, out string runCondition);
                    string info = $"## {Name} Technical Information";
                    info += $"Behavior ID: {Id}\r\n";
                    info += $"Behavior Type: {BehaviorMode}\r\n";
                    if (_pollRateSeconds >= 0)
                    {
                        info += $"Execution Cycle: {_pollRateSeconds} seconds\r\n";
                    }
                    info += $"Required Point Tags: \r\n";
                    foreach (var tag in requiredTags)
                    {
                        info += $"-{tag}\r\n";
                    }

                    info += $"Optional Point Tag: \r\n";
                    foreach (var tag in optionalTags)
                    {
                        info += $"-{tag}\r\n";
                    }

                    info += "\r\n";

                    info += $"Run Condition: {runCondition}";
                    info += "\r\n";

                    info += "### Mapped Entities:\r\n";
                    if (Parent != null)
                    {
                        info += $"Parent: {Parent.GetProperty<string>(BrickSchema.Net.EntityProperties.PropertiesEnum.Name)}\r\n";
                        info += $"Required Point Tag: \r\n";
                        foreach (var tag in requiredTags)
                        {
                            Point? p = Parent.GetPointEntity(tag);
                            info += $"- [{p != null}] {tag}\r\n";
                        }
                        info += $"Optional Point Tag: \r\n";
                        foreach (var tag in optionalTags)
                        {
                            Point? p = Parent.GetPointEntity(tag);
                            info += $"- [{p != null}] {tag}\r\n";
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

        protected void Execute(object? sender, EventArgs e)
        {
            //this function is dedicated for event call back
            _executeByTimer = false; 
            Execute();
        }

        protected virtual bool IsBehaviorRunnable()
        {
            return false;
        }
        protected virtual BehaviorTaskReturnCodes ProcessTask(out List<BehaviorValue> behaviorValues)
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
        protected virtual BehaviorTaskReturnCodes GenerateInsight(out string insight)
        {
            insight = "Not Implimented.";
            return BehaviorTaskReturnCodes.NotImplemented;
        }

        protected virtual BehaviorTaskReturnCodes GenerateResolution(out string resolution)
        {
            resolution = "Not Implimented.";
            return BehaviorTaskReturnCodes.NotImplemented;
        }

        protected virtual void Load()    {      }

        protected virtual void Unload() { }

        #endregion default Functions

        


        public BrickEntity? AskAssociatedWith(params dynamic[] args) {  
            
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
            foreach ( var point in pointOfs )
            {
                if ( point != null )
                {
                    var behaviors = point.Behaviors.Where(x => x.Type?.Equals(type) ?? false);
                    behaviors.Except(args);
                }
            }
            return null;
        }
    }
}
