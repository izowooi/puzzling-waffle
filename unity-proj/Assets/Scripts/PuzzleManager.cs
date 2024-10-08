using System;
using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    private int _currentPuzzleIndex;
    private int _currentPuzzleDifficulty;
    private static PuzzleManager _instance;
    public static PuzzleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PuzzleManager>();
                if (_instance == null)
                {
                    Debug.LogError("예상하지 못한 에러가 발생했습니다.");
                    GameObject go = new GameObject("PuzzleManager");
                    _instance = go.AddComponent<PuzzleManager>();
                }
            }
            return _instance;
        }
    }
    // 스프라이트 렌더러 리스트를 저장할 변수
    private List<SpriteRenderer> puzzlePieceSpriteRenderers;
    private List<PuzzlePiece> puzzlePieces;
    public static Action<int, int, float> OnClearPuzzle;
    //List<PuzzlePiecePosition> pieces
    public static Action<int, int, List<PuzzlePiecePosition>> OnPuzzlePiecePlaced;
    
    [SerializeField]
    private Transform puzzlePieceParent4by4;
    void Awake()
    {
        Debug.Log("PuzzleManager Awake() 호출");
        // 싱글톤 인스턴스 설정
        if (_instance == null)
        {
            PuzzlePiece.OnRightPosition += OnRightPosition;
            _instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 유지
        }
        else
        {
            Destroy(gameObject); // 중복된 인스턴스 제거
        }

        // 리스트 초기화
        puzzlePieceSpriteRenderers = new List<SpriteRenderer>();
        puzzlePieces = new List<PuzzlePiece>();

        // 현재 게임 오브젝트의 모든 자식들을 확인
        foreach (Transform child in puzzlePieceParent4by4)
        {
            // 자식의 태그가 "puzzle piece"인지 확인
            if (child.CompareTag("PuzzlePiece"))
            {
                var puzzlePiece = child.GetComponent<PuzzlePiece>();
                if (puzzlePiece != null)
                {
                    puzzlePieces.Add(puzzlePiece);
                }
                
                // 해당 자식의 자식을 가져옴 (각 자식 오브젝트에는 하나의 자식이 있다고 가정)
                foreach (Transform grandChild in child)
                {
                    // 자식의 자식에서 SpriteRenderer 컴포넌트를 가져옴
                    SpriteRenderer spriteRenderer = grandChild.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        // 리스트에 추가
                        puzzlePieceSpriteRenderers.Add(spriteRenderer);
                    }
                }
            }
        }
    }
    
    private List<PuzzlePiecePosition> getRightPositions()
    {
        List<PuzzlePiecePosition> rightPositions = new List<PuzzlePiecePosition>();
        foreach (var puzzlePiece in puzzlePieces)
        {
            if(puzzlePiece.InRightPosition)
                rightPositions.Add(puzzlePiece.GridPosition);
        }

        return rightPositions;
    }

    private bool IsCleared()
    {
        foreach (var puzzlePiece in puzzlePieces)
        {
            if (!puzzlePiece.InRightPosition)
            {
                return false;
            }
        }

        return true;

    }
    private void OnRightPosition()
    {
        bool isCleared = IsCleared();
        var rightPositions = getRightPositions(); 
        if (isCleared)
        {
            OnClearPuzzle?.Invoke(_currentPuzzleIndex, _currentPuzzleDifficulty, 0f);   
        }
        else
        {
            OnPuzzlePiecePlaced?.Invoke(_currentPuzzleIndex, _currentPuzzleDifficulty, rightPositions);
        }
    }

    public void LoadPuzzle(int puzzleIndex)
    {
        _currentPuzzleIndex = puzzleIndex;
        //_currentPuzzleDifficulty = 4;
        _currentPuzzleDifficulty = 16;
        Sprite newSprite = SpriteManager.Instance.GetSpriteByIndex(_currentPuzzleIndex);
        
        if (newSprite != null)
        {
            ChangePuzzlePieceSprites(newSprite);
        }

        MoveRandomPuzzlePiece();
    }

    // 스프라이트를 변경하는 메서드
    public void ChangePuzzlePieceSprites(Sprite newSprite)
    {
        foreach (var spriteRenderer in puzzlePieceSpriteRenderers)
        {
            spriteRenderer.sprite = newSprite;
        }
    }
    
    private void MoveRandomPuzzlePiece()
    {
        PuzzleProgressInfo progressInfo = StageDataManager.Instance.GetStageInfo(_currentPuzzleIndex, _currentPuzzleDifficulty);
        foreach (var puzzlePiece in puzzlePieces)
        {
            if (progressInfo != null && progressInfo.Pieces.Contains(puzzlePiece.GridPosition))
                continue;
            
            puzzlePiece.MoveToRandomPosition();
        }
    }

    public void EndGame()
    {
        MoveRightPosition();
        Show(false);
    }
    public void Show(bool isShow)
    {
        transform.localScale = isShow ? Vector3.one : Vector3.zero;
    }

    public void MoveRightPosition()
    {
        foreach (var puzzlePiece in puzzlePieces)
        {
            puzzlePiece.MoveToRightPosition();
        }
    }
}