namespace Shahkar.Provider.Dto
{
    public class ShahkarGetPersonInfoResponseDto
    {

        public BasicInformationDto basicInformation { get; set; }
        public IdentificationInformationDto identificationInformation { get; set; }
        public RegistrationStatusDto registrationStatus { get; set; }
        public OfficeInformationDto officeInformation { get; set; }
        public ResponseContextDto responseContext { get; set; }


        public class BasicInformationDto
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string fatherName { get; set; }
            public string gender { get; set; }

        }

        public class IdentificationInformationDto
        {
            public string nationalId { get; set; }
            public string birthDate { get; set; }
            public string shenasnameSeri { get; set; }
            public string shenasnameSerial { get; set; }
            public string shenasnamehNumber { get; set; }
        }

        public class RegistrationStatusDto
        {
            public string deathStatus { get; set; }
        }

        public class OfficeInformationDto
        {
            public int officeCode { get; set; }
            public string officeName { get; set; }
        }

        public class ResponseContextDto
        {
            public StatusDto status { get; set; }
            public string requestId { get; set; }
            public string correlationId { get; set; }
            public string navigationURI { get; set; }
            public string nextStepToken { get; set; }
            public string userSessionId { get; set; }
            public object custom { get; set; }
        }

        public class StatusDto
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<object> details { get; set; }
        }
    }
}


