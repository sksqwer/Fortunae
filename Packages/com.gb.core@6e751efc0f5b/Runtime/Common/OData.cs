namespace GB
{
    public class OData<T> : IOData
    {
        private T _value;

        public OData(T value)
        {
            this._value = value;
        }

        public T Get()
        {
            return _value;
        }

        public void Set(T value)
        {
            _value = value;
        }
    }
    public static class ODataWrapperUtility
    {
        public static T Get<T>(this IOData data)
        {
            return ODataConverter.Convert<T>(data);
        }

        public static T OConvert<T>(this IOData data)
        {
            return ODataConverter.Convert<T>(data);
        }
    }



    public interface IOData {}

    public class ODataConverter
    {
        public static T Convert<T>(IOData iOdata)
        {
            OData<T> data = (OData<T>)iOdata;
            return data.Get();
        }
    }
}

