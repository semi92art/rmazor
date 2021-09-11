public interface IInit
{
    event NoArgsHandler Initialized;
    void Init();
}

public interface IPreInit
{
    event NoArgsHandler PreInitialized;
    void PreInit();
}

public interface IPostInit
{
    event NoArgsHandler PostInitialized;
    void PostInit();
}