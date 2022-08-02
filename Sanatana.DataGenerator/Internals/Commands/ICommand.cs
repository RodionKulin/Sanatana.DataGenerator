namespace Sanatana.DataGenerator.Internals.Commands
{
    public interface ICommand
    {
        void Execute();
        string GetLogEntry();
    }
}