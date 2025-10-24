using System.Text;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class AnimatorStateDebugger : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator; // se deixar vazio, pega automaticamente no Reset/Awake

    [Header("Options")]
    public int layer = 0;                // Base Layer
    public bool logTransitions = true;   // loga quando o estado muda
    public bool logParameters = false;   // loga valores dos parâmetros quando muda de estado
    public bool showOverlay = true;      // mostra um HUD simples na tela
    public KeyCode toggleOverlayKey = KeyCode.F3;

    // cache
    int _lastHash = -1;
    string _lastClipName = "";
    GUIStyle _style;

    void Reset()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        _style = new GUIStyle
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };
    }

    void Update()
    {
        if (!animator) return;

        // toggle overlay
        if (Input.GetKeyDown(toggleOverlayKey))
            showOverlay = !showOverlay;

        var st = animator.GetCurrentAnimatorStateInfo(layer);
        string clipName = GetActiveClipName(animator, layer, st);
        int hash = st.shortNameHash;

        if (hash != _lastHash)
        {
            _lastHash = hash;
            _lastClipName = clipName;

            if (logTransitions)
            {
                Debug.Log($"[Animator] Layer {layer} → Estado: {_lastClipName} (hash {hash})  " +
                          $"normalizedTime={st.normalizedTime:0.00}  length≈{GetActiveClipLength(animator, layer):0.00}s");
            }

            if (logParameters)
            {
                Debug.Log("[Animator/Params] " + DumpParameters(animator));
            }
        }
    }

    // ---------- helpers ----------
    static string GetActiveClipName(Animator anim, int layer, AnimatorStateInfo st)
    {
        var infos = anim.GetCurrentAnimatorClipInfo(layer);
        if (infos != null && infos.Length > 0 && infos[0].clip != null)
            return infos[0].clip.name;
        // fallback: hash
        return st.shortNameHash.ToString();
    }

    static float GetActiveClipLength(Animator anim, int layer)
    {
        var infos = anim.GetCurrentAnimatorClipInfo(layer);
        if (infos != null && infos.Length > 0 && infos[0].clip != null)
            return infos[0].clip.length;
        return 0f;
    }

    static string DumpParameters(Animator anim)
    {
        var sb = new StringBuilder();
        var ps = anim.parameters;
        for (int i = 0; i < ps.Length; i++)
        {
            var p = ps[i];
            switch (p.type)
            {
                case AnimatorControllerParameterType.Float:
                    sb.Append($"{p.name}={anim.GetFloat(p.name):0.###}");
                    break;
                case AnimatorControllerParameterType.Int:
                    sb.Append($"{p.name}={anim.GetInteger(p.name)}");
                    break;
                case AnimatorControllerParameterType.Bool:
                    sb.Append($"{p.name}={anim.GetBool(p.name)}");
                    break;
                case AnimatorControllerParameterType.Trigger:
                    sb.Append($"{p.name}=<Trigger>");
                    break;
            }
            if (i < ps.Length - 1) sb.Append("  |  ");
        }
        return sb.ToString();
    }

    // ---------- simple HUD ----------
    void OnGUI()
    {
        if (!showOverlay || !animator) return;

        var p = new Vector2(12, 12);
        var padY = 20;

        GUI.Label(new Rect(p.x, p.y, 800, 20), $"Animator (Layer {layer}) — Estado: {_lastClipName}", _style); p.y += padY;

        var ps = animator.parameters;
        if (ps.Length > 0)
        {
            GUI.Label(new Rect(p.x, p.y, 1000, 20), "Parâmetros:", _style); p.y += padY;
            foreach (var par in ps)
            {
                string val = par.type switch
                {
                    AnimatorControllerParameterType.Float => animator.GetFloat(par.name).ToString("0.###"),
                    AnimatorControllerParameterType.Int => animator.GetInteger(par.name).ToString(),
                    AnimatorControllerParameterType.Bool => animator.GetBool(par.name).ToString(),
                    AnimatorControllerParameterType.Trigger => "<Trigger>",
                    _ => "?"
                };
                GUI.Label(new Rect(p.x + 12, p.y, 1000, 20), $"{par.name}: {val}", _style);
                p.y += padY;
            }
        }
        else
        {
            GUI.Label(new Rect(p.x, p.y, 800, 20), "Sem parâmetros no Animator.", _style);
        }
    }
}
