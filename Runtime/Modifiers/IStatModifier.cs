namespace StatController.Runtime
{
    public interface IStatModifier
    {
        public StatModifierType modifierType
        {
            get;
            set;
        }

        public string identity
        {
            get;
            set;
        }

        public int priority
        {
            get;
            set;
        }

        protected float rightOperand
        {
            get;
            set;
        }
        
        
        public float Calculate(float value);
    }
}