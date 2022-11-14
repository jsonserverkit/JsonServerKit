using JsonServerKit.AppServer.Data.Crud;
// ReSharper disable UnusedTypeParameter

namespace JsonServerKit.AppServer.Operations
{
    /// <summary>
    /// Defines the types T, TCreate, TRead, TUpdate, TDelete.
    /// Requires TCreate, TRead, TUpdate, TDelete to be of specific types from namespace JsonServerKit.AppServer.Data.Crud.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TCreate"></typeparam>
    /// <typeparam name="TRead"></typeparam>
    /// <typeparam name="TUpdate"></typeparam>
    /// <typeparam name="TDelete"></typeparam>
    public interface ICrudOperation<T, TCreate, TRead, TUpdate, TDelete>
        where TCreate : Create<T>
        where TRead : Read<T>
        where TUpdate : Update<T>
        where TDelete : Delete<T>
    {
    }
}
