namespace Onwo.Threading.Tasks
{
    public interface IInitialiser
    {
        NamedLazyTask Init { get; set; }
    }
}
