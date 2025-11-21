# 🎮 Ari

**Gênero:** Plataforma 2D de Ação
**Engine:** Unity 6000.0.59f2
**Plataforma:** PC (Windows/Mac/Linux)

---

## 📖 Sobre o Jogo

**Ari** é um jogo de plataforma 2D de ação com temática medieval dark fantasy. O jogador embarca em uma jornada através de diferentes ambientes - da floresta ao calabouço, até o confronto final no castelo. O jogo combina exploração, combate com projéteis, sistema de poções e desafios de plataforma.

A narrativa é contada através de cutscenes entre os níveis, guiando o jogador através de uma história épica até a batalha final contra um poderoso chefe.

---

## ✨ Características Principais

- **Sistema de Movimento Fluido** - Corrida, pulo com coyote time, escalada de escadas e plataformas atravessáveis
- **Combate com Projéteis** - Atire continuamente e desbloqueie ataques especiais
- **5 Tipos de Poções** - Cura, velocidade, pulo extra, ataque especial e congelamento
- **Power-ups Permanentes** - Desbloqueie habilidades especiais como pulo alto e ataque extra
- **Variedade de Inimigos** - Enfrente dragões, aranhas, olhos flutuantes, jinns e caveiras explosivas
- **Boss com Múltiplas Fases** - Batalha final épica com 3 fases e ataques variados
- **Armadilhas Ambientais** - Espinhos, machados giratórios, plantas carnívoras e ácido
- **Sistema de Saúde Visual** - Barra de vida com feedback imediato
- **Cutscenes Narrativas** - História imersiva entre os níveis

---

## 🎯 Controles

| Ação | Controle |
|------|----------|
| **Mover** | ⬅️ ➡️ Setas ou `A` `D` |
| **Pular** | `Espaço` |
| **Correr** | `Shift` (segurar) |
| **Subir/Descer Escada** | ⬆️ ⬇️ Setas ou `W` `S` |
| **Atravessar Plataforma** | `S` ou `⬇️` |
| **Atirar** | `Botão Esquerdo do Mouse` (segurar) |
| **Ataque Extra** | `E` (requer power-up) |
| **Pulo Alto** | `Q` (requer power-up) |

---

## 🧪 Sistema de Poções

As poções são coletáveis que fornecem efeitos temporários ou instantâneos:

| Tipo | Efeito | Duração |
|------|--------|---------|
| 🩸 **Saúde** | Restaura 1 ponto de vida | Instantâneo |
| ⚡ **Velocidade** | Aumenta velocidade de movimento em 50% | 5 segundos |
| 🦘 **Pulo Extra** | Aumenta força do pulo em 50% | 5 segundos |
| ⚔️ **Ataque Extra** | Desbloqueia ataque especial (tecla E) | Temporário |
| ❄️ **Congelamento** | Congela todos os inimigos em 5 unidades de raio | 3 segundos |

**Efeitos Visuais:** Todas as poções possuem animações de rotação, partículas ao coletar e som de pickup.

---

## 👾 Inimigos e Boss

### Inimigos Regulares
- **🐉 Dragão** - Inimigo voador com ataques à distância
- **👁️ Olho** - Criatura flutuante com projéteis mágicos
- **🕷️ Aranha** - Inimigo terrestre com alcance de perseguição
- **🧞 Jinn** - Entidade mágica com ataques ranged
- **💀 Caveira Explosiva** - Armadilha que explode próximo ao jogador (2 de dano, raio de 2 unidades)

### Boss Final
- **3 Fases Progressivas** - Dificuldade aumenta a cada fase
- **3 Tipos de Ataque:**
  - **Melee** - Ataque corpo a corpo de curto alcance
  - **Fogo** - Projétil mágico de fogo
  - **Raio** - Ataque especial de longo alcance
- Sistema de cooldown adaptativo que acelera nas fases finais

---

## 🛠️ Requisitos Técnicos

### Versão do Unity
- **Unity 6000.0.59f2** ou superior

### Principais Pacotes
- Universal Render Pipeline (URP) 17.0.4
- Input System 1.14.2
- 2D Animation, Sprite, Tilemap
- Cinemachine
- Timeline
- TextMesh Pro

### Especificações Recomendadas
- **Resolução:** 1920x1080
- **Sistema Operacional:** Windows 10/11, macOS, ou Linux
- **Espaço em Disco:** ~200 MB

---

## 🚀 Como Executar o Projeto

1. **Clone o repositório:**
   ```bash
   git clone <url-do-repositorio>
   cd Ari
   ```

2. **Abra o projeto no Unity Hub:**
   - Versão recomendada: Unity 6000.0.59f2
   - Selecione a pasta raiz do projeto

3. **Aguarde a importação dos assets**

4. **Selecione a cena inicial:**
   - Abra `Assets/Scenes/Canva_Menu.unity` para começar pelo menu
   - Ou abra `Assets/Scenes/Level_0_Design.unity` para testar o gameplay direto

5. **Pressione Play** no editor do Unity

---

## 📁 Estrutura do Projeto

```
Ari/
├── Assets/
│   ├── Animations/          # Animações de personagens e objetos
│   ├── Prefabs/             # 60+ prefabs (inimigos, itens, armadilhas)
│   ├── Scenes/              # 12 cenas (menu, níveis, cutscenes)
│   ├── Scripts/             # 98 scripts C# (lógica do jogo)
│   ├── Sounds/              # Músicas e efeitos sonoros
│   ├── Sprites/             # Assets visuais organizados por categoria
│   ├── UI/                  # Interface e imagens de cutscenes
│   └── Settings/            # Configurações do Unity (URP, Input, etc.)
├── Packages/                # Dependências e pacotes
└── ProjectSettings/         # Configurações do projeto
```

### Cenas Principais
- **Canva_Menu.unity** - Menu principal
- **Level_0_Design.unity** - Primeiro nível (tutorial)
- **Level_1_Design.unity** - Segundo nível
- **Final_Level.unity** - Nível do boss
- **Cutscene_Forest/Dungeon/Castle/Victory.unity** - Narrativa
- **EndGameScene.unity** - Tela de game over

---

## 🎨 Características Técnicas

### Sistemas Implementados
- **Sistema de Input** - Usando o novo Input System da Unity
- **Gerenciamento de Estado** - Máquinas de estado para player, inimigos e boss
- **Sistema de Áudio** - SfxManager centralizado para efeitos sonoros
- **Efeitos Visuais** - Partículas, parallax, animações e flickering de luz
- **Sistema de Cutscenes** - Timeline integration com efeitos typewriter
- **Física 2D** - Detecção de chão, plataformas one-way, escadas
- **Sistema de UI** - Barra de vida, menus, tutoriais e transições

### Mecânicas Avançadas
- **Coyote Time** - 0.12s de graça para pular após sair de plataformas
- **Jump Buffer** - Buffering de input para pulos mais responsivos
- **Alinhamento Vertical** - Inimigos só atacam quando alinhados verticalmente
- **Sistema de Congelamento** - Inimigos mudam de cor para azul claro quando congelados
- **Respawn de Armadilhas** - Espinhos e outros perigos se regeneram automaticamente

---

## 👥 Créditos

**Desenvolvimento:** [Adicione os nomes da equipe aqui]

**Disciplina:** Jogos e Interações - 5º Semestre

**Instituição:** [Adicione o nome da instituição]

---

## 📝 Notas de Desenvolvimento

Este projeto foi desenvolvido como parte do curso de Jogos e Interações. O repositório está em desenvolvimento ativo com melhorias contínuas.

### Histórico Recente
- ✅ Sistema de barra de vida do jogador
- ✅ Implementação completa do sistema de poções
- ✅ Integração de cutscenes narrativas
- ✅ Sistema de UI e feedback visual

---

## 📄 Licença

[Adicione a licença apropriada para o projeto]

---

**Divirta-se jogando Ari!** 🎮✨
