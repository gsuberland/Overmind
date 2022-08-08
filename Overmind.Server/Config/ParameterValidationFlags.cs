using log4net;

namespace Overmind.Server.Config
{
    [Flags]
    enum ParameterValidationFlags
    {
        FileMustExist = 1,
    }
}