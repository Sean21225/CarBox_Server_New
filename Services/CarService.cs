using CarboxBackend.Models;
using CarboxBackend.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace CarboxBackend.Services
{
    public class CarService
    {
        private readonly CarRepository _carRepository;
        private readonly RouteRepository _routeRepository;

        public CarService(CarRepository carRepository, RouteRepository routeRepository)
        {
            _carRepository = carRepository;
            _routeRepository = routeRepository;
        }

        /// <summary>
        /// עדכון התחנה האחרונה שהרכב עבר בה לפי המיקום שלו במסלול.
        /// </summary>
        /// <param name="carNumber">מספר הזיהוי של הרכב</param>
        public async Task UpdateLastStationAsync(string carNumber)
        {
            // 🔹 שליפת הרכב מהמסד לפי מספר הרכב
            var car = await _carRepository.GetCarByIdAsync(carNumber);
            if (car == null)
                return; // אם הרכב לא קיים, אין צורך להמשיך

            // 🔹 שליפת המסלול של הרכב מהמסד על פי RouteId
            var route = await _routeRepository.GetRouteByIdAsync(car.RouteId);
            if (route == null)
                return; // אם המסלול לא קיים, אין צורך להמשיך

            // 🔹 בדיקת המרחק של התחנה האחרונה שהרכב עבר מהתחנה הראשית (0)
            double lastStationDistance = route.DistancesFromChargingStation.ContainsKey(car.LastStation.Id)
                ? route.DistancesFromChargingStation[car.LastStation.Id] // אם קיימת תחנה קודמת, קבל את המרחק שלה
                : 0; // אחרת, נניח שהרכב נמצא בתחנה הראשית (טעינה)

            // 🔹 חיפוש התחנה הבאה שהרכב עבר על פניה
            var nextStation = route.Stations
                .Where(s => route.DistancesFromChargingStation.ContainsKey(s.Id) && // לוודא שהתחנה קיימת במילון המרחקים
                            route.DistancesFromChargingStation[s.Id] > lastStationDistance && // לוודא שהרכב עבר אותה
                            route.DistancesFromChargingStation[s.Id] <= route.DistancesFromChargingStation.Values.Max()) // לוודא שהיא לא מחוץ לטווח התחנות
                .OrderBy(s => route.DistancesFromChargingStation[s.Id]) // סידור התחנות לפי המרחק
                .FirstOrDefault(); // לקיחת התחנה הקרובה ביותר שהרכב עבר עליה

            // 🔹 אם הרכב עבר את כל התחנות וחזר להתחלה, יש לאפס אותו לתחנת הטעינה
            if (nextStation == null && car.LastStation == route.Stations.Last())
            {
                car.LastStation = route.ChargingStation; // חזרה לתחנה 0 (טעינה)
            }
            else if (nextStation != null)
            {
                car.LastStation = nextStation; // עדכון התחנה האחרונה לתחנה החדשה שנמצאה
            }

            // 🔹 שמירת העדכון במסד הנתונים
            await _carRepository.UpdateCarAsync(car);
        }
    }
}
