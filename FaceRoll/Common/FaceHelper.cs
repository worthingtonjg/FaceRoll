using FaceRoll.Model;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using static Windows.UI.Xaml.Media.Imaging.WriteableBitmapExtensions;

namespace FaceRoll.Common
{
    public class FaceHelper
    {
        private readonly IFaceClient _faceClient;

        public FaceHelper(string subscriptionKey, string apiRoot)
        {
            _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey), new DelegatingHandler[] { })
            {
                BaseUri = new Uri(apiRoot)
            };
        }

        public async Task InitFaceGroup(string id, string name = null)
        {
            if(name == null)
            {
                name = id;
            }

            try
            {
                PersonGroup group = await _faceClient.PersonGroup.GetAsync(id);
            }
            catch (APIErrorException ex)
            {
                if (ex.Body.Error.Code == "PersonGroupNotFound")
                {
                    await _faceClient.PersonGroup.CreateAsync(id, name);
                    await _faceClient.PersonGroup.TrainAsync(id);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IList<DetectedFace>> Detect(StorageFile file, IList<FaceAttributeType> faceAttributes = null)
        {
            if (faceAttributes == null)
            {
                faceAttributes = new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };
            }

            using (var stream = await file.OpenStreamForReadAsync())
            {
                var faces = await _faceClient.Face.DetectWithStreamAsync(stream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);

                Debug.WriteLine(JsonConvert.SerializeObject(faces, Formatting.Indented));

                return faces;
            }
        }

        public async Task<WriteableBitmap> MarkFaces(StorageFile file, DetectedFace[] faces)
        {
            if (faces.Length == 0) return null;

            using (var stream = await file.OpenStreamForReadAsync())
            {
                WriteableBitmap wb = await BitmapFactory.FromStream(stream);
                using (wb.GetBitmapContext())
                {

                    for (int i = 0; i < faces.Length; ++i)
                    {
                        DetectedFace face = faces[i];

                        wb.DrawRectangle(
                            face.FaceRectangle.Left,
                            face.FaceRectangle.Top,
                            face.FaceRectangle.Left + face.FaceRectangle.Width,
                            face.FaceRectangle.Top + face.FaceRectangle.Height,
                            Colors.Red
                            );
                    }
                }

                return wb;
            }
        }

        public async Task<List<Identification>> Identify(string personGroupId, StorageFile file)
        {
            var result = new List<Identification>();

            try
            {
                var faces = await Detect(file);

                var faceIds = faces.Select(face => face.FaceId.GetValueOrDefault()).ToList();

                if (faceIds.Count == 0)
                {
                    Debug.WriteLine("No Faces Found");
                    return result;
                }

                TrainingStatusType status = await IsTrainingComplete(personGroupId);

                IList<IdentifyResult> identities = new List<IdentifyResult>();
                if (status != TrainingStatusType.Failed)
                {

                    identities = await _faceClient.Face.IdentifyAsync(personGroupId, faceIds);
                }

                foreach (var face in faces)
                {
                    var identifyResult = identities.Where(i => i.FaceId == face.FaceId).FirstOrDefault();

                    var identification = new Identification
                    (
                        person: new Person { Name = "Unknown" },
                        confidence: 1,
                        face: face,
                        identifyResult: identifyResult
                    );

                    result.Add(identification);

                    if (identifyResult != null && identifyResult.Candidates.Count > 0)
                    {

                        // Get top 1 among all candidates returned
                        IdentifyCandidate candidate = identifyResult.Candidates[0];

                        var person = await _faceClient.PersonGroupPerson.GetAsync(personGroupId, candidate.PersonId);

                        identification.Person = person;
                        identification.Confidence = candidate.Confidence;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return result;
        }

        public async Task<IList<PersonGroup>> ListGroups()
        {
            var groups = await _faceClient.PersonGroup.ListAsync();

            return groups;
        }

        public async Task CreatePersonGroup(string personGroupId, string personGroupName)
        {
            await _faceClient.PersonGroup.CreateAsync(personGroupId, personGroupName);
        }

        public async Task DeletePersonGroup(string personGroupId)
        {
            await _faceClient.PersonGroup.DeleteAsync(personGroupId);
        }

        public async Task<IList<Person>> GetPersonsInGroup(string personGroupId)
        {
            var persons = await _faceClient.PersonGroupPerson.ListAsync(personGroupId);
            
            return persons;
        }

        public async Task<Person> AddPerson(string personGroupId, string personName)
        {
            Person result = await _faceClient.PersonGroupPerson.CreateAsync(personGroupId, personName);

            return result;
        }

        public async Task DeletePerson(string personGroupId, Guid personId)
        {
            await _faceClient.PersonGroupPerson.DeleteAsync(personGroupId, personId);
        }

        public async Task AddImageToPerson(string personGroupId, Guid personId, StorageFile file)
        {
            using (var s = await file.OpenStreamForReadAsync())
            {
                await _faceClient.PersonGroupPerson.AddPersonFaceFromStreamAsync(personGroupId, personId, s);
            }
        }

        public async Task<Person> GetPerson(string personGroupId, Guid personId)
        {
            var person = await _faceClient.PersonGroupPerson.GetAsync(personGroupId, personId);

            return person;
        }

        public async Task TrainGroup(string personGroupId)
        {
            await _faceClient.PersonGroup.TrainAsync(personGroupId);
        }

        public async Task<TrainingStatusType> IsTrainingComplete(string personGroupId)
        {
            TrainingStatus status = await _faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);

            return status.Status;
        }
    }
}
