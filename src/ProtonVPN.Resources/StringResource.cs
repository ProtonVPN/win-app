namespace ProtonVPN.Resource
{
    public class StringResource
    {
        public static string Get(string key)
        {
            return Properties.Strings.ResourceManager.GetString(key);
        }
    }
}
