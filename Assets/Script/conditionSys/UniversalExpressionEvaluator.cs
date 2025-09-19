using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting; // Flow
using Unity.VisualScripting.Dependencies.NCalc; // VS 内置 NCalc

// 用法要点：
// 1) 先 SetExpression 或在 Inspector 写好 inlineExpression，再 Build()（Awake 会自动 Build）。
// 2) 每次判断时调用 EvaluateBool(...)，把变量/函数以“解析器委托”或“字典”传进来。
// 3) 解析器本身不依赖任何 mover/door/任务系统，真正通用。

public class UniversalExpressionEvaluator : MonoBehaviour
{
    [Header("表达式来源（两选一）")]
    [TextArea] public string inlineExpression = "true"; // 直接在 Inspector 写
    public bool useInline = true;

    [Tooltip("可选：从外部脚本设置/热更（SetExpression 会覆盖 inlineExpression）")]
    public string runtimeOverrideExpression;

    private Expression _expr;
    private bool _valid;

    // 当前一次 Evaluate 的解析器（委托），由调用方提供
    private Func<string, object> _varResolver;
    private Func<string, object[], object> _funcResolver;

    void Awake()
    {
        Build();
    }

    public void SetExpression(string expr)
    {
        runtimeOverrideExpression = expr;
        useInline = false;
        Build();
    }

    public void Build()
    {
        string text = useInline ? inlineExpression : runtimeOverrideExpression;
        _valid = false;
        _expr = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning($"{name}: Expression is empty.");
            return;
        }

        try
        {
            _expr = new Expression(text, EvaluateOptions.IgnoreCase);
            _expr.EvaluateParameter += OnEvaluateParameter;   // 变量
            _expr.EvaluateFunction  += OnEvaluateFunction;    // 函数

            // 预热一次（VS 的 NCalc 需要传 Flow）
            var r = _expr.Evaluate((Flow)null);
            _valid = r is bool || r is int || r is double;
            if (!_valid)
                Debug.LogWarning($"{name}: Expression returns {r?.GetType().Name}, not bool/int/double.");
        }
        catch (Exception e)
        {
            Debug.LogError($"{name}: Build expression failed: {e.Message}\n{text}");
            _expr = null;
        }
    }

    // 方案一：使用“变量解析委托 + 函数解析委托”
    // varResolver: 输入变量名 -> 返回变量值（bool/int/double/string 等）
    // funcResolver: 输入函数名+实参 -> 返回函数结果
    public bool EvaluateBool(Func<string, object> varResolver,
                             Func<string, object[], object> funcResolver = null)
    {
        if (!_valid || _expr == null) return false;

        _varResolver = varResolver;
        _funcResolver = funcResolver;

        try
        {
            var r = _expr.Evaluate((Flow)null);
            return r switch
            {
                bool b   => b,
                int i    => i != 0,
                double d => Math.Abs(d) > double.Epsilon,
                _        => false
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"{name}: Evaluate error: {e.Message}");
            return false;
        }
        finally
        {
            // 避免闭包持有外部对象
            _varResolver = null;
            _funcResolver = null;
        }
    }

    // 方案二：使用字典（方便直接传入一批变量/函数）
    private readonly Dictionary<string, object> _varsCache = new();
    private readonly Dictionary<string, Func<object[], object>> _funcsCache = new();

    public bool EvaluateBool(IDictionary<string, object> variables,
                             IDictionary<string, Func<object[], object>> functions = null)
    {
        // 用委托包装成方案一，避免重复实现
        _varsCache.Clear();
        if (variables != null)
            foreach (var kv in variables) _varsCache[kv.Key] = kv.Value;

        _funcsCache.Clear();
        if (functions != null)
            foreach (var kv in functions) _funcsCache[kv.Key] = kv.Value;

        object VarResolver(string name)
        {
            // 忽略大小写支持
            if (_varsCache.TryGetValue(name, out var v)) return v;
            var lower = name.ToLowerInvariant();
            if (_varsCache.TryGetValue(lower, out v)) return v;
            return false; // 未知变量默认 false
        }

        object FuncResolver(string fname, object[] args)
        {
            if (_funcsCache.TryGetValue(fname, out var f)) return f(args);
            var lower = fname.ToLowerInvariant();
            if (_funcsCache.TryGetValue(lower, out f)) return f(args);
            return false; // 未知函数默认 false
        }

        return EvaluateBool(VarResolver, FuncResolver);
    }

    // —— NCalc 回调 —— //
    private void OnEvaluateParameter(Flow flow, string name, ParameterArgs args)
    {
        if (_varResolver != null)
            args.Result = _varResolver(name);
        else
            args.Result = false; // 没提供解析器则默认 false
    }

    private void OnEvaluateFunction(Flow flow, string name, FunctionArgs args)
    {
        if (_funcResolver != null)
        {
            var ps = new object[args.Parameters.Length];
            for (int i = 0; i < ps.Length; i++)
                ps[i] = args.Parameters[i].Evaluate(flow);
            args.Result = _funcResolver(name, ps);
        }
        else
        {
            args.Result = false;
        }
    }
}
