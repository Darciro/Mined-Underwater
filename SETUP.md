# Setup e regras do repositório

## Primeira configuração (cada máquina)

```sh
brew install git-lfs
git lfs install          # uma vez por máquina, ANTES de clonar/fazer pull
git clone git@github.com:Darciro/Mined-Underwater.git
cd Mined-Underwater && git lfs pull
```

Sem Git LFS, imagens/áudio/modelos chegam como ficheiros de texto de 130 bytes e o Unity importa-os partidos.

## Regras

1. **Feche o Unity antes de `git pull`, `checkout`, `reset` ou de mexer em pastas fora do Editor.**
   Com o Unity aberto, ele regista GUIDs a meio da operação e o cache em `Library/` fica inconsistente
   (erros `CS0246` em massa nos scripts de Editor dos plugins).
2. **Pacotes da Asset Store vivem em `Assets/_Imports/`** (Feel, Epic Toon FX, Layer Lab, 2D Green Turtle, Underwater_pack)
   e o Text Animator em `Assets/Plugins/Febucci/`. Nunca copie as pastas dos pacotes à mão para `Assets/`.
3. **Atualize/importe pacotes só pelo Package Manager → My Assets → Import.** O Unity atualiza os ficheiros no
   lugar onde já estão (por GUID). Depois de atualizar, faça commit logo, para as outras máquinas receberem
   a mesma versão (incluindo as `.dll`, que agora são versionadas).
4. **Não faça commit com a Console a mostrar erros de compilação.**

## Verificação automática

`Assets/Scripts/Editor/ProjectHealthCheck.cs` corre ao abrir o Editor (e em *Tools → Project Health Check*)
e dá erro se encontrar ficheiros LFS por descarregar ou pacotes duplicados fora de `_Imports`.

## Se aparecerem centenas de erros depois de um pull

1. Feche o Unity.
2. `git status` — se houver `.meta` modificados em massa em `_Imports`, reponha: `git checkout -- Assets/_Imports`.
3. `git lfs pull`
4. Apague a pasta `Library/` e reabra o projeto (reimportação completa).
