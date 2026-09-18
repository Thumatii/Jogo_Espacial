using UnityEngine;
using UnityEngine.SceneManagement;

public static class DadosGlobais
{
    // Memória da sessão no Espaço
    public static bool jaEntrouNoEspaco = false;
    public static Vector3 posicaoSalvaDaNave;
    public static Vector2 velocidadeVetorSalva;
    public static float velocidadeAtualSalva;
    public static float tempoSaida;

    // Órbita
    public static bool estaEmOrbita = false;
    public static string nomePlanetaOrbitado = "";
    public static float anguloOrbitaSalvo = 0f;

    // Memória da cena Interior
    public static Vector3 posicaoJogadorInterior;
    public static bool temPosicaoInteriorSalva = false;

    // Flag de controle para tempo congelado ao fechar/abrir o jogo
    public static bool carregouDoQuit = false;

    // ===== SALVAR NO DISCO (PLAYERPREFS) =====
    public static void SalvarNoDisco()
    {
        PlayerPrefs.SetString("CenaSalva", SceneManager.GetActiveScene().name);

        // Dados da Nave (Espaço)
        PlayerPrefs.SetFloat("NavePosX", posicaoSalvaDaNave.x);
        PlayerPrefs.SetFloat("NavePosY", posicaoSalvaDaNave.y);
        PlayerPrefs.SetFloat("NavePosZ", posicaoSalvaDaNave.z);
        PlayerPrefs.SetInt("EstaEmOrbita", estaEmOrbita ? 1 : 0);
        PlayerPrefs.SetString("NomePlanetaOrbitado", nomePlanetaOrbitado);
        PlayerPrefs.SetFloat("AnguloOrbita", anguloOrbitaSalvo);

        // Dados do Jogador (Interior)
        PlayerPrefs.SetFloat("JogadorIntX", posicaoJogadorInterior.x);
        PlayerPrefs.SetFloat("JogadorIntY", posicaoJogadorInterior.y);
        PlayerPrefs.SetFloat("JogadorIntZ", posicaoJogadorInterior.z);
        PlayerPrefs.SetInt("TemPosicaoInterior", temPosicaoInteriorSalva ? 1 : 0);

        PlayerPrefs.SetInt("TemSave", 1);
        PlayerPrefs.Save();
    }

    // ===== CARREGAR DO DISCO =====
    public static bool CarregarDoDisco()
    {
        if (PlayerPrefs.GetInt("TemSave", 0) == 0) return false;

        // Carrega posição da Nave
        posicaoSalvaDaNave = new Vector3(
            PlayerPrefs.GetFloat("NavePosX"),
            PlayerPrefs.GetFloat("NavePosY"),
            PlayerPrefs.GetFloat("NavePosZ")
        );

        velocidadeVetorSalva = Vector2.zero;
        velocidadeAtualSalva = 0f;

        estaEmOrbita = PlayerPrefs.GetInt("EstaEmOrbita") == 1;
        nomePlanetaOrbitado = PlayerPrefs.GetString("NomePlanetaOrbitado", "");
        anguloOrbitaSalvo = PlayerPrefs.GetFloat("AnguloOrbita", 0f);

        // Carrega posição do Jogador no Interior
        posicaoJogadorInterior = new Vector3(
            PlayerPrefs.GetFloat("JogadorIntX"),
            PlayerPrefs.GetFloat("JogadorIntY"),
            PlayerPrefs.GetFloat("JogadorIntZ")
        );
        temPosicaoInteriorSalva = PlayerPrefs.GetInt("TemPosicaoInterior", 0) == 1;

        jaEntrouNoEspaco = true;
        carregouDoQuit = true;

        return true;
    }

    // ===== OBJETOS INSPECIONADOS (descobertos) =====
    private const string PrefixoInspecionado = "Inspecionado_";

    public static bool ObjetoJaFoiInspecionado(string idUnico)
    {
        if (string.IsNullOrEmpty(idUnico)) return false;
        return PlayerPrefs.GetInt(PrefixoInspecionado + idUnico, 0) == 1;
    }

    public static void MarcarObjetoInspecionado(string idUnico)
    {
        if (string.IsNullOrEmpty(idUnico)) return;
        PlayerPrefs.SetInt(PrefixoInspecionado + idUnico, 1);
        PlayerPrefs.Save();
    }

    public static void DesmarcarObjetoInspecionado(string idUnico)
    {
        if (string.IsNullOrEmpty(idUnico)) return;
        PlayerPrefs.DeleteKey(PrefixoInspecionado + idUnico);
        PlayerPrefs.Save();
    }

    // ===== APARÊNCIA ESCOLHIDA (troca de sprite) =====
    private const string PrefixoAparencia = "Aparencia_";

    public static int CarregarAparenciaEscolhida(string idUnico, int padrao)
    {
        if (string.IsNullOrEmpty(idUnico)) return padrao;
        return PlayerPrefs.GetInt(PrefixoAparencia + idUnico, padrao);
    }

    public static void SalvarAparenciaEscolhida(string idUnico, int indice)
    {
        if (string.IsNullOrEmpty(idUnico)) return;
        PlayerPrefs.SetInt(PrefixoAparencia + idUnico, indice);
        PlayerPrefs.Save();
    }
}