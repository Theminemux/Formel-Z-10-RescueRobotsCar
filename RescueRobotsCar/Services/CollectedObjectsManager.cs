namespace RescueRobotsCar.Services
{
    public class CollectedObjectsManager
    {
        private List<string> collectedObjects;
        public IReadOnlyList<string> CollectedObjects => collectedObjects.AsReadOnly();

        public CollectedObjectsManager()
        {
            collectedObjects = [];
        }

        public void AddCollectedObject(string objectName)
        {
            if (!collectedObjects.Contains(objectName))
            {
                collectedObjects.Add(objectName);
                Console.WriteLine($"Object collected: {objectName}");
            }
        }
    }
}
