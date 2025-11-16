namespace API_painel_investimentos.Repositories.Profile.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IInvestorProfileRepository InvestorProfiles { get; }
        IProfileQuestionRepository ProfileQuestions { get; }

        Task<int> CommitAsync();
        Task RollbackAsync();
    }
}
