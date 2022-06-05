namespace Sanatana.DataGenerator.Commands
{
    public interface ICommand
    {
        void Execute();
        string GetDescription();
    }
}