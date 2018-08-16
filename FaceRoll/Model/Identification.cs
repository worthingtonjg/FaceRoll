using Microsoft.ProjectOxford.Face.Contract;

namespace FaceRoll.Model
{
    public class Identification
    {
        public Person Person { get; set; }
        public IdentifyResult IdentifyResult { get; set; }
        public Face Face { get; set; }

        public double Confidence;

        public Identification(string name, double confidence, IdentifyResult identifyResult, Face face)
        {
            Person = new Person();
            Person.Name = name;
            IdentifyResult = identifyResult;
            Face = face;
            Confidence = confidence;
        }

        public Identification(Person person, double confidence, IdentifyResult identifyResult, Face face)
        {
            Person = person;
            IdentifyResult = identifyResult;
            Face = face;
            Confidence = confidence;
        }
    }
}
