namespace RescueRobotsCar.Services
{
    public class LineSensorsMidpoint
    {
        /// <summary>
        /// Berechnet die Mittelpunktposition der Liniensensoren.
        /// -1 = ganz links, 0 = Mitte, 1 = ganz rechts
        /// </summary>
        public static double CalculateMidpoint(int[] sensorValues)
        {
            if (sensorValues == null || sensorValues.Length == 0)
                return 0;

            // Positionen der 5 Sensoren: -1, -0.5, 0, 0.5, 1
            double[] positions = { -1, -0.5, 0, 0.5, 1 };

            double weightedSum = 0;
            int totalSum = 0;

            for (int i = 0; i < sensorValues.Length && i < positions.Length; i++)
            {
                weightedSum += sensorValues[i] * positions[i];
                totalSum += sensorValues[i];
            }

            // Wenn keine Sensoren aktiviert sind, Mitte zurückgeben
            if (totalSum == 0)
                return 0;

            return weightedSum / totalSum;
        }
    }
}
