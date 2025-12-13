using System.Text.Json.Serialization;
using EcaInventoryApi.Common;

namespace EcaInventoryApi.Model
{
    [JsonConverter(typeof(SnakeCaseEnumJsonConverter<ReservationStatus>))]
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Canceled,
        Released
    }
}
