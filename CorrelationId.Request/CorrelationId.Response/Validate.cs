using Serilog;

namespace CorrelationId.Response
{
    public static class Validate
    {
        public static bool Validated(bool valid)
        {
            if (valid)
            {
                Log.Information("Dados válidos!");
                return true;
            }
            else
            {
                Log.Error("Dados inválidos!");                                
                return false;
            }
        }
    }
}
