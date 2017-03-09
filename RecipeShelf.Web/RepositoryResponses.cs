namespace RecipeShelf.Web
{
    public struct AllResponse
    {
        public readonly string[] Ids;

        public readonly string Error;

        public AllResponse(string error)
        {
            Ids = null;
            Error = error;
        }

        public AllResponse(string[] ids)
        {
            Ids = ids;
            Error = null;
        }
    }

    public struct CreateResponse
    {
        public readonly string NewId;

        public readonly string Error;

        public CreateResponse(string error, string newId = null)
        {
            Error = error;
            NewId = newId;
        }
    }
}
