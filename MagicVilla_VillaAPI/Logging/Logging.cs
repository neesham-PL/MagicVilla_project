namespace MagicVilla_VillaAPI.Logging
{
    public class Logging
    {
        public void Log(string message , string type)
        {
            if(type == "error")
            {
                Console.WriteLine("ERROR - " + message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}
