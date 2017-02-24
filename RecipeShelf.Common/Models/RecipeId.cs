namespace RecipeShelf.Common.Models
{
    public struct RecipeId
    {
        private readonly string Value;

        public RecipeId(string value)
        {
            Value = value;
        }

        public static implicit operator RecipeId(string id)
        {
            return id != null ? new RecipeId(id) : null;
        }

        public static implicit operator string(RecipeId id)
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
            return obj is RecipeId ? ((RecipeId)obj).Value.Equals(Value)
                            : obj is string ? ((string)obj).Equals(Value)
                                    : false;
        }
    }
}
