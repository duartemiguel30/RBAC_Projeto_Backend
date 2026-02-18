using System.ComponentModel.DataAnnotations;

namespace WorkplaceTasks.API.Models;

public enum TaskStatus
{
    Pendente = 0,
    EmProgresso = 1,
    Concluido = 2
}

public class TaskItem
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Range(0, 2, ErrorMessage = "O status deve ser: 0 (Pendente), 1 (Em Progresso) ou 2 (Concluído).")]
    public TaskStatus Status { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}