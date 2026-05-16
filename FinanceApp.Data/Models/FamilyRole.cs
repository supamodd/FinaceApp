using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceApp.Data.Models;

public enum FamilyRole
{
    [Display(Name = "Владелец")]
    Owner = 0,

    [Display(Name = "Администратор")]
    Admin = 1,

    [Display(Name = "Участник")]
    Member = 2,

    [Display(Name = "Наблюдатель")]
    Viewer = 3
}
