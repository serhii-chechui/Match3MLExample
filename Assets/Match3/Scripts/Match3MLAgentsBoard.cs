using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine;

public class Match3MLAgentsBoard : AbstractBoard
{
    [SerializeField]
    private Match3 _match3;
    
    [SerializeField]
    private Match3Visual _match3Visual;

    private LevelSO _levelSo;

    private Agent _agent;

    private BoardSize _boardSize;
    
    private void Awake()
    {
        _levelSo = _match3.GetLevelSO();
        
        _boardSize = new BoardSize
        {
            Columns = _levelSo.width,
            Rows = _levelSo.height,
            NumCellTypes = _levelSo.gemList.Count,
            NumSpecialTypes = _levelSo.goalType == LevelSO.GoalType.Score ? 0 : 1
        };

        _agent = GetComponent<Agent>();
        
        _match3Visual.OnStateChanged += Match3VisualOnOnStateChanged;
        _match3.OnGemGridPositionDestroyed += Match3OnOnGemGridPositionDestroyed;
        _match3.OnGlassDestroyed += Match3OnOnGlassDestroyed;
        _match3.OnMoveUsed += Match3OnOnMoveUsed;
        _match3.OnOutOfMoves += Match3OnOnOutOfMoves;
        _match3.OnWin += Match3OnOnWin;
    }

    private void Match3OnOnWin(object sender, EventArgs e)
    {
        _agent.EndEpisode();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void Match3OnOnOutOfMoves(object sender, EventArgs e)
    {
        _agent.EndEpisode();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void Match3OnOnMoveUsed(object sender, EventArgs e)
    {
        if (_levelSo.goalType == LevelSO.GoalType.Glass)
        {
            _agent.AddReward(-0.5f);
        }
    }

    private void Match3OnOnGlassDestroyed(object sender, EventArgs e)
    {
        if (_levelSo.goalType == LevelSO.GoalType.Glass)
        {
            _agent.AddReward(1f);
        }
    }

    private void Match3OnOnGemGridPositionDestroyed(object sender, EventArgs e)
    {
        if (_levelSo.goalType == LevelSO.GoalType.Score)
        {
            _agent.AddReward(1f);
        }
    }

    private void Match3VisualOnOnStateChanged(object sender, EventArgs e)
    { 
        var state = _match3Visual.GetState();
        
        switch (state)
        {
            case Match3Visual.State.Busy:
                break;
            case Match3Visual.State.WaitingForUser:
                _agent.RequestDecision();
                break;
            case Match3Visual.State.TryFindMatches:
                break;
            case Match3Visual.State.GameOver:
                break;
        }
    }

    public override BoardSize GetMaxBoardSize()
    {
        _levelSo = _match3.GetLevelSO();
        
        return new BoardSize
        {
            Columns = _levelSo.width,
            Rows = _levelSo.height,
            NumCellTypes = _levelSo.gemList.Count,
            NumSpecialTypes = _levelSo.goalType == LevelSO.GoalType.Score ? 0 : 1
        };
    }
    
    public override BoardSize GetCurrentBoardSize()
    {
        return _boardSize;
    }

    public override int GetCellType(int row, int col)
    {
        var gemSO = _match3.GetGemSO(col, row);
        return _levelSo.gemList.IndexOf(gemSO);
    }

    public override int GetSpecialType(int row, int col)
    {
        // return _match3.HasGlass(col, row) ? 1 : 0;
        return 0;
    }

    public override bool IsMoveValid(Move m)
    {
        var startX = m.Column;
        var startY = m.Row;
        
        var moveEnd = m.OtherCell();

        var endX = moveEnd.Column;
        var endY = moveEnd.Row;

        return _match3.CanSwapGridPositions(startX, startY, endX, endY);
    }

    public override bool MakeMove(Move m)
    {
        var startX = m.Column;
        var startY = m.Row;
        
        var moveEnd = m.OtherCell();

        var endX = moveEnd.Column;
        var endY = moveEnd.Row;

        if (_match3.CanSwapGridPositions(startX, startY, endX, endY))
        {
            //Can Make Move
            _match3Visual.SwapGridPositions(startX, startY, endX, endY);
            return true;
        }
        else
        {
            //Can't Make Move
            return false;
        }
    }
}
