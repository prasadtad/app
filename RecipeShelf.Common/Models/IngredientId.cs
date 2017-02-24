namespace RecipeShelf.Common.Models
{
    public struct IngredientId
    {
        private readonly string Value;

        public IngredientId(string value)
        {
            Value = value;
        }

        public static implicit operator IngredientId(string id)
        {
            return id != null ? new IngredientId(id) : null;
        }

        public static implicit operator string(IngredientId id)
        {
            return id.Value;
        }        

        public override string ToString()
        {
            return Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is IngredientId ? ((IngredientId)obj).Value.Equals(Value)
                            : obj is string ? ((string)obj).Equals(Value)
                                    : false;
        }
    }
}
