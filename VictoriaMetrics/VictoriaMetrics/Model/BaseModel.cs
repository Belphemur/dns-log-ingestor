namespace VictoriaMetrics.VictoriaMetrics.Model
{
    public abstract class BaseModel<TValue>
    {
        public string Name  { get; set; }
        public TValue Value { get; set; }
    }
}