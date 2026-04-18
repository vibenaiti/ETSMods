# ETSMods — V Rising BepInEx Mod Suite

Coleção de mods para V Rising, construídos sobre BepInEx 6 + HarmonyX + VampireCommandFramework.  
Todos os mods compartilham o **ModCore** como biblioteca central de utilitários ECS.

---

## Mods

| Mod | Descrição |
|---|---|
| **ModCore** | Biblioteca central: helpers ECS, modelos de dados, serviços e eventos compartilhados |
| **Achievements** | Sistema de conquistas baseado em kills de VBlood e eventos do servidor |
| **KillFeed** | Feed de mortes PvP no chat, com rastreio de killer/victim e Soul Shards |
| **GatedProgression** | Trava bosses VBlood até pré-requisitos serem cumpridos (progressão em cadeia) |
| **ShopMod** | Loja in-game com zonas de compra, preços configuráveis e proteção de itens no death |
| **PointsMod** | Sistema de pontos com ranking, recompensas e integração com outros mods |
| **RaidConfig** | Configuração de horários de raid, gestão de Shard drops e controle de clãs |
| **VipMod** | Sistema VIP com whitelist, slots reservados e permissões especiais |
| **QuickStash** | Auto-deposit de inventário em todos os baús do castelo com um comando |
| **Moderation** | Ferramentas de moderação: kick, ban, mute, gestão de clãs |
| **OpenWorldEvents** | Eventos no mundo aberto: BossContest, FlashDuel e modos de jogo customizados |
| **InstabreachGolems** | Golens de breach instântaneo — modifica prefabs de golem para breach sem delay |
| **Notify** | Sistema de notificações de servidor (login/logout, eventos, broadcasts) |
| **DiscordBot** | Integração com Discord: relay de chat, notificações de eventos e status do servidor |
| **ChipSaMods** | Kits de itens, spectate de jogadores e utilitários de admin |
| **DebugMod** | Ferramentas de debug: waypoints, inspeção de inventário, testes de componentes ECS |
| **LoggingMod** | Logging estruturado de eventos do servidor para arquivo |
| **TemplateMod** | Template base para novos mods (estrutura padrão do projeto) |

---

## Arquitetura

Todos os mods dependem do **ModCore**, que provê:

- `ModCore.Helpers` — acesso ao `EntityManager`, prefabs, jogadores
- `ModCore.Models` — `Player`, `DamageRecord` e outros modelos de domínio
- `ModCore.Services` — serviços de spawn, inventário, buff
- `ModCore.Events` — sistema de eventos interno entre mods
- `ModCore.Data` — dados estáticos: `Prefabs`, `BloodData`, `JewelData`, `LegendaryData`

### Stack

- **C# 10 / netstandard2.1**
- **BepInEx 6.x** (Unity IL2CPP)
- **HarmonyX 2.x**
- **VampireCommandFramework** — comandos de chat
- **Unity DOTS ECS** — todo estado do jogo vive em Entities + Components
- **ProjectM** — assembly nativo do V Rising

---

## Estrutura do Repositório

```
ETSMods/
├── ModCore/              # Biblioteca central (dependência de todos os mods)
│   ├── Data/             # Prefabs, BloodData, JewelData, LegendaryData, Rotations
│   ├── Helpers/          # Acesso ao ECS, jogadores, inventário
│   ├── Models/           # Player, DamageRecord, etc.
│   ├── Services/         # Spawn, buff, loot services
│   └── Events/           # Sistema de eventos entre mods
│
├── Achievements/         # Sistema de conquistas (VBlood kills)
├── ChipSaMods/           # Kits, spectate, utilitários admin
├── DebugMod/             # Ferramentas de debug e waypoints
├── DiscordBot/           # Integração Discord
├── GatedProgression/     # Progressão em cadeia de bosses
├── InstabreachGolems/    # Modificação de prefabs de golem
├── KillFeed/             # Feed PvP de mortes no chat
├── LoggingMod/           # Logging de eventos para arquivo
├── Moderation/           # Ferramentas de moderação
├── Notify/               # Sistema de notificações
├── OpenWorldEvents/      # Eventos no mundo: BossContest, FlashDuel
├── PointsMod/            # Sistema de pontos e ranking
├── QuickStash/           # Auto-deposit em baús do castelo
├── RaidConfig/           # Configuração de raids e Shards
├── ShopMod/              # Loja in-game com zonas
├── TemplateMod/          # Template para novos mods
└── VipMod/               # Sistema VIP e whitelist
```

---

## Build

```bash
# Build de um mod específico
cd ModCore && dotnet build
cd KillFeed && dotnet build

# Build de toda a solution
dotnet build ETSMods.sln
```

### Dependências de referência

As DLLs de referência (`ProjectM`, `Unity.Entities`, etc.) devem estar em:
```
~/Desktop/my/VRisingDedicatedServer/BepInEx/interop/
```

---

## Padrões ECS Comuns

### Iterar entidades com componente
```csharp
var query = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
var entities = query.ToEntityArray(Allocator.Temp);
try
{
    foreach (var e in entities) { /* ... */ }
}
finally { entities.Dispose(); }
```

### Verificar existência antes de acessar componente
```csharp
if (!em.Exists(entity)) return;
if (!em.HasComponent<Health>(entity)) return;
var health = em.GetComponentData<Health>(entity);
```

---

## Licença

MIT
