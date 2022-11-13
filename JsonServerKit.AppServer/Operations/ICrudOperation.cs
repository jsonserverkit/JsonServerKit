using JsonServerKit.AppServer.Data.Crud;

namespace JsonServerKit.AppServer.Operations
{
    public interface ICrudOperation<T, TCreate, TRead, TUpdate, TDelete> : IOperation
        where TCreate : Create<T>
        where TRead : Read<T>
        where TUpdate : Update<T>
        where TDelete : Delete<T>
    {
    }
}
