public class FaceAttendanceRequest
{
    public string Matricule { get; set; }
    public string CapturedImageBase64 { get; set; }
    public Guid HeaderId { get; set; } // L'ID de la fiche de pointage actuelle
    public int Day { get; set; } // Le numéro du jour (1 à 31)
}