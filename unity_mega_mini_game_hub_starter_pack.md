# Unity Mega Mini‑Game Hub (shared coins) — Starter Pack

This starter gives you a **modular hub** that can host many simple training/arcade mini‑games, all sharing one coin economy. You can ship with a few games now and keep adding more modules later.

---

## 1) Project structure
```
Assets/
  Scripts/
    Core/
      Boot.cs
      SceneNames.cs
      ServiceLocator.cs
      SaveService.cs
      EconomyService.cs
      EventBus.cs
    Catalog/
      MiniGameDef.cs
      GameRegistry.cs
    Hub/
      HomeController.cs
      MiniGameLoader.cs
    Contracts/
      IMinigame.cs
      GameResult.cs
      IMinigameContext.cs
    UI/
      CoinHUD.cs
  AddressableAssetsData/   (after you enable Addressables)
  Resources/
    Registry/
      GameRegistry.asset
  MiniGames/
    TapTrainer/
      TapTrainer.prefab
      TapTrainer.cs
    LaneRunner/
      LaneRunner.prefab
      LaneRunner.cs
Scenes/
  Boot.unity
  Home.unity
  MiniGame.unity
```

---

## 2) Core services

### `ServiceLocator.cs`
```csharp
using System;
using UnityEngine;

public static class ServiceLocator {
    static EconomyService _economy;
    static SaveService _save;
    static EventBus _bus;

    public static void Init() {
        _save ??= new SaveService();
        _economy ??= new EconomyService(_save);
        _bus ??= new EventBus();
    }

    public static EconomyService Economy => _economy;
    public static SaveService Save => _save;
    public static EventBus Bus => _bus;
}
```

### `SaveService.cs` (simple PlayerPrefs JSON)
```csharp
using UnityEngine;

[System.Serializable]
public class SaveData { public int coins = 0; }

public class SaveService {
    const string KEY = "mega_hub_save_v1";
    SaveData _data;

    public SaveService(){ Load(); }

    public SaveData Data => _data;

    public void Load(){
        if(PlayerPrefs.HasKey(KEY)){
            var json = PlayerPrefs.GetString(KEY);
            _data = JsonUtility.FromJson<SaveData>(json);
        } else {
            _data = new SaveData();
            Save();
        }
    }

    public void Save(){
        var json = JsonUtility.ToJson(_data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }
}
```

### `EconomyService.cs`
```csharp
using System;

public class EconomyService {
    readonly SaveService _save;
    public event Action<int> OnCoinsChanged;

    public EconomyService(SaveService save){ _save = save; }

    public int Coins => _save.Data.coins;

    public void AddCoins(int amount){
        if(amount <= 0) return;
        _save.Data.coins += amount;
        _save.Save();
        OnCoinsChanged?.Invoke(_save.Data.coins);
    }

    public bool SpendCoins(int amount){
        if(amount <= 0) return true;
        if(_save.Data.coins < amount) return false;
        _save.Data.coins -= amount;
        _save.Save();
        OnCoinsChanged?.Invoke(_save.Data.coins);
        return true;
    }
}
```

### `EventBus.cs` (lightweight pub/sub)
```csharp
using System;

public class EventBus {
    public event Action<GameResult> OnMiniGameFinished;
    public void PublishMiniGameFinished(GameResult r) => OnMiniGameFinished?.Invoke(r);
}
```

---

## 3) Contracts for mini‑game modules

### `IMinigame.cs`, `GameResult.cs`, `IMinigameContext.cs`
```csharp
using UnityEngine;

public interface IMinigame {
    void Init(IMinigameContext ctx);
    void StartGame();
    void StopGame();
}

[System.Serializable]
public class GameResult {
    public string gameId;
    public int score;
    public int coinsAwarded;
    public float duration;
}

public interface IMinigameContext {
    void ReportResult(GameResult result);
}
```

---

## 4) Mini‑game catalog (ScriptableObjects)

### `MiniGameDef.cs`
```csharp
using UnityEngine;

[CreateAssetMenu(menuName="MegaHub/MiniGameDef")]
public class MiniGameDef : ScriptableObject {
    public string gameId;
    public string displayName;
    public Sprite icon;
    [Tooltip("Addressables key or Resources path to prefab implementing IMinigame")]
    public string prefabKey;
    [Range(0,100)] public int baseCoinReward = 10;
}
```

### `GameRegistry.cs`
```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="MegaHub/GameRegistry")]
public class GameRegistry : ScriptableObject {
    public List<MiniGameDef> games = new();

    public MiniGameDef GetById(string id) => games.Find(g => g.gameId == id);
}
```

> Create one `GameRegistry.asset` under `Resources/Registry/` and list all your `MiniGameDef` assets.

---

## 5) Scenes & flow

### `SceneNames.cs`
```csharp
public static class SceneNames {
    public const string Boot = "Boot";
    public const string Home = "Home";
    public const string MiniGame = "MiniGame";
}
```

### `Boot.cs` (attach to an empty in Boot scene)
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour {
    void Awake(){
        Application.targetFrameRate = 60;
        ServiceLocator.Init();
        SceneManager.LoadScene(SceneNames.Home);
    }
}
```

### `HomeController.cs` (bind to Home scene root)
```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeController : MonoBehaviour {
    public GameRegistry registry;
    public Transform gridRoot; // parent for buttons
    public Button buttonPrefab; // simple UI button prefab
    public Text coinText;

    void Start(){
        RefreshCoins();
        ServiceLocator.Economy.OnCoinsChanged += _ => RefreshCoins();
        foreach(var def in registry.games){
            var b = Instantiate(buttonPrefab, gridRoot);
            b.GetComponentInChildren<Text>().text = def.displayName;
            b.onClick.AddListener(()=> StartMiniGame(def.gameId));
        }
    }

    void RefreshCoins(){ coinText.text = $"Coins: {ServiceLocator.Economy.Coins}"; }

    void StartMiniGame(string gameId){
        PlayerPrefs.SetString("_pending_game", gameId);
        SceneManager.LoadScene(SceneNames.MiniGame);
    }
}
```

### `MiniGameLoader.cs` (in MiniGame scene)
```csharp
using UnityEngine;
using UnityEngine.UI;

public class MiniGameLoader : MonoBehaviour, IMinigameContext {
    public GameRegistry registry;
    public Transform gameRoot;
    public Button quitButton;

    IMinigame _current;
    string _gameId;

    void Start(){
        _gameId = PlayerPrefs.GetString("_pending_game", string.Empty);
        var def = registry.GetById(_gameId);
        if(def == null){ UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Home); return; }

        // For simplicity, load from Resources. (You can switch to Addressables later.)
        var prefab = Resources.Load<GameObject>(def.prefabKey);
        var go = Instantiate(prefab, gameRoot);
        _current = go.GetComponent<IMinigame>();
        _current.Init(this);
        _current.StartGame();

        quitButton.onClick.AddListener(()=>{
            _current?.StopGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Home);
        });
    }

    public void ReportResult(GameResult r){
        // reward coins
        ServiceLocator.Economy.AddCoins(r.coinsAwarded);
    }
}
```

---

## 6) Example mini‑games

### A) **Tap Trainer** — timing taps when a moving marker hits target

**Hierarchy idea**: a Canvas with a ring `Target` and a rotating `Needle`. On tap, score if angle within window.

`TapTrainer.cs`
```csharp
using UnityEngine;
using UnityEngine.UI;

public class TapTrainer : MonoBehaviour, IMinigame {
    public Text scoreText;
    public Image needle;
    public RectTransform targetArc; // set start/end via editor or rotate sprite
    public float speed = 180f; // deg/sec
    public float allowedWindow = 20f; // +/- degrees
    public int rounds = 10;

    IMinigameContext _ctx;
    int _score;
    int _taps;
    float _angle;
    bool _running;

    public void Init(IMinigameContext ctx){ _ctx = ctx; }

    public void StartGame(){ _running = true; _score = 0; _taps = 0; }
    public void StopGame(){ _running = false; }

    void Update(){
        if(!_running) return;
        _angle = ( _angle + speed * Time.deltaTime ) % 360f;
        needle.rectTransform.localEulerAngles = new Vector3(0,0,-_angle);
        if(Input.GetMouseButtonDown(0)) HandleTap();
    }

    void HandleTap(){
        _taps++;
        float targetAngle = targetArc.localEulerAngles.z;
        float delta = Mathf.DeltaAngle(_angle, targetAngle);
        if(Mathf.Abs(delta) <= allowedWindow){ _score += 10; }
        scoreText.text = $"Score: {_score}";
        if(_taps >= rounds) Finish();
    }

    void Finish(){
        _running = false;
        var result = new GameResult{ gameId = "tap_trainer", score = _score, coinsAwarded = Mathf.Max(5, _score/5), duration = rounds/speed };
        _ctx.ReportResult(result);
    }
}
```

Create a prefab `TapTrainer.prefab` in `Resources/TapTrainer/` and set `prefabKey` of its `MiniGameDef` to `TapTrainer/TapTrainer`.

### B) **Lane Runner** — swipe left/right to dodge blocks (very tiny prototype)

`LaneRunner.cs`
```csharp
using UnityEngine;

public class LaneRunner : MonoBehaviour, IMinigame {
    public Transform player;
    public Transform[] lanes; // 0..N-1 lane x positions
    public float speedZ = 6f;
    public GameObject obstaclePrefab;

    IMinigameContext _ctx; int _laneIndex=1; float _z;
    float _spawnZ; bool _running; int _score;

    public void Init(IMinigameContext ctx){ _ctx = ctx; }
    public void StartGame(){ _running = true; _score=0; _z=0; _spawnZ=10; }
    public void StopGame(){ _running = false; }

    void Update(){
        if(!_running) return;
        _z += speedZ * Time.deltaTime;
        player.position = new Vector3(lanes[_laneIndex].position.x, player.position.y, _z);
        if(Input.GetKeyDown(KeyCode.LeftArrow)) _laneIndex = Mathf.Max(0,_laneIndex-1);
        if(Input.GetKeyDown(KeyCode.RightArrow)) _laneIndex = Mathf.Min(lanes.Length-1,_laneIndex+1);
        if(Input.GetMouseButtonDown(0)){
            var x = Input.mousePosition.x / Screen.width;
            _laneIndex = x < 0.5f ? Mathf.Max(0,_laneIndex-1) : Mathf.Min(lanes.Length-1,_laneIndex+1);
        }
        if(_z > _spawnZ){
            _spawnZ += 8f;
            var lane = Random.Range(0, lanes.Length);
            Instantiate(obstaclePrefab, new Vector3(lanes[lane].position.x, 0, _spawnZ), Quaternion.identity);
            _score += 5;
        }
    }

    void OnTriggerEnter(Collider other){
        if(!_running) return;
        _running = false;
        _ctx.ReportResult(new GameResult{ gameId="lane_runner", score=_score, coinsAwarded=Mathf.Max(5,_score/2)});
    }
}
```

Create a simple scene space with 3 lane markers (empty transforms) and a capsule player that has a collider; obstacles are cubes with colliders set as triggers to call `OnTriggerEnter`.

---

## 7) Coin HUD (shared on Home & MiniGame)

`CoinHUD.cs`
```csharp
using UnityEngine;
using UnityEngine.UI;

public class CoinHUD : MonoBehaviour {
    public Text coinText;
    void OnEnable(){
        Refresh();
        ServiceLocator.Economy.OnCoinsChanged += _=> Refresh();
    }
    void OnDisable(){ ServiceLocator.Economy.OnCoinsChanged -= _=> Refresh(); }
    void Refresh(){ coinText.text = $"Coins: {ServiceLocator.Economy.Coins}"; }
}
```

---

## 8) Hooking up the catalog
1. Create `MiniGameDef` for each game: `Tap Trainer` (id: `tap_trainer`, key: `TapTrainer/TapTrainer`) and `Lane Runner`.
2. Add them to `GameRegistry.asset`.
3. In **Home** scene, add `HomeController`, assign `registry`, `gridRoot`, `buttonPrefab`, and a `Text` for coin display.
4. In **MiniGame** scene, add `MiniGameLoader`, set `registry` and `gameRoot`.

---

## 9) Going modular for real (Addressables)
- Replace `Resources.Load` with Addressables (`Addressables.LoadAssetAsync<GameObject>(def.prefabKey)`), mark each mini‑game prefab as Addressable.
- You can later ship new games by pushing only Addressables content updates.

---

## 10) IAP & ads (shared coin packs)
- Use **Unity IAP** (Google Play Billing) for `1000_coins`, `5000_coins`, `remove_ads`. On purchase success: `ServiceLocator.Economy.AddCoins(amount)`.
- Rewarded ads: grant small coin bonuses across the whole hub.

---

## 11) Policy & settings (Android)
- Build **App Bundle (.aab)**, **Scripting Backend: IL2CPP**, **ARM64**.
- Target **API 35** (Android 15) in Player Settings; Min SDK 24+.

---

## 12) Add a new mini‑game (workflow)
1) Duplicate a game folder under `MiniGames/YourGame/`.
2) Implement `IMinigame` in a single MonoBehaviour.
3) Create prefab, make it Addressable, note its key.
4) Create a `MiniGameDef` asset and add to `GameRegistry`.

That’s it — it appears on the Home grid and shares the same coins.
