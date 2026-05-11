using CasaMulher.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Aluna> Alunas => Set<Aluna>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Presenca> Presencas => Set<Presenca>();
    public DbSet<FichaAtendimento> FichasAtendimento => Set<FichaAtendimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Aluna>(entity =>
        {
            entity.Property(aluna => aluna.Nome).HasMaxLength(120).IsRequired();
            entity.Property(aluna => aluna.Cpf).HasMaxLength(14);
            entity.Property(aluna => aluna.Telefone).HasMaxLength(20);
            entity.Property(aluna => aluna.Email).HasMaxLength(120);
        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.Property(curso => curso.Nome).HasMaxLength(120).IsRequired();
            entity.Property(curso => curso.Descricao).HasMaxLength(500);
        });

        modelBuilder.Entity<Turma>(entity =>
        {
            entity.Property(turma => turma.Nome).HasMaxLength(120).IsRequired();
            entity.Property(turma => turma.DiasSemana).HasMaxLength(120).IsRequired();
            entity.Property(turma => turma.Local).HasMaxLength(120);
            entity.Property(turma => turma.Responsavel).HasMaxLength(120).IsRequired();

            entity.HasOne(turma => turma.Curso)
                .WithMany(curso => curso.Turmas)
                .HasForeignKey(turma => turma.CursoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Aula>(entity =>
        {
            entity.Property(aula => aula.Conteudo).HasMaxLength(500);

            entity.HasOne(aula => aula.Turma)
                .WithMany(turma => turma.Aulas)
                .HasForeignKey(aula => aula.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Matricula>(entity =>
        {
            entity.Property(matricula => matricula.Status).HasMaxLength(30).IsRequired();
            entity.HasIndex(matricula => new { matricula.AlunaId, matricula.TurmaId }).IsUnique();

            entity.HasOne(matricula => matricula.Aluna)
                .WithMany(aluna => aluna.Matriculas)
                .HasForeignKey(matricula => matricula.AlunaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(matricula => matricula.Turma)
                .WithMany(turma => turma.Matriculas)
                .HasForeignKey(matricula => matricula.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Presenca>(entity =>
        {
            entity.Property(presenca => presenca.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(presenca => presenca.Observacao).HasMaxLength(500);
            entity.HasIndex(presenca => new { presenca.AulaId, presenca.MatriculaId }).IsUnique();

            entity.HasOne(presenca => presenca.Aula)
                .WithMany(aula => aula.Presencas)
                .HasForeignKey(presenca => presenca.AulaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(presenca => presenca.Matricula)
                .WithMany(matricula => matricula.Presencas)
                .HasForeignKey(presenca => presenca.MatriculaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FichaAtendimento>(entity =>
        {
            entity.Property(ficha => ficha.TipoAtendimento).HasMaxLength(120);
            entity.Property(ficha => ficha.Observacoes).HasMaxLength(1000);
            entity.Property(ficha => ficha.ValidadaPor).HasMaxLength(120);
            entity.Property(ficha => ficha.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.HasOne(ficha => ficha.Aluna)
                .WithMany(aluna => aluna.FichasAtendimento)
                .HasForeignKey(ficha => ficha.AlunaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
