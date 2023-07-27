using System.Collections;
using Data.Entities.Activities;

namespace Service.Models.Activity;

public class FirebaseAttendanceModel
{
    public String Title { get; set; }

    public ICollection<FirebaseAttendanceItem> Items { get; set; }
}

