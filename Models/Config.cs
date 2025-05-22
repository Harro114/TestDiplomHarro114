using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Models;

public class Config
{

    public string Name { get; set; }
    public string? ValueString { get; set; }
    public DateTime? ValueDate { get; set; }
    public int? ValueInt { get; set; }
    public float? ValueFloat { get; set; }
}