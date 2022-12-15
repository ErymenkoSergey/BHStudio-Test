namespace HBStudio.Test.Interface
{
    public interface ICharacterable
    {
        bool GetIsInvincibilityMode();
        void CmdInvincibilityStatusOn();
        string GetName();
    }
}