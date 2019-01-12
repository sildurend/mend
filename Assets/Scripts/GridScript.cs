﻿using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridScript : MonoBehaviour
{
    /* Inspector */

    #region INSPECTOR

    [SerializeField]
    private GameObject edgePrefab = null;

    [SerializeField]
    private GameObject topLeftEdgePrefab = null;

    [SerializeField]
    private GameObject topMiddleEdgePrefab = null;

    [SerializeField]
    private GameObject topRightEdgePrefab = null;

    [SerializeField]
    private GameObject bottomLeftEdgePrefab = null;

    [SerializeField]
    private GameObject bottomMiddleEdgePrefab = null;

    [SerializeField]
    private GameObject bottomRightEdgePrefab = null;

    [SerializeField]
    private GameObject middleRightEdgePrefab = null;

    [SerializeField]
    private GameObject middleLeftEdgePrefab = null;

    [SerializeField]
    private GameObject[] tilePrefabs = null;

    [SerializeField]
    private Ease MovementEase = Ease.Unset;

    [SerializeField]
    private Ease MoveToMiddleEase = Ease.Unset;

    [SerializeField]
    private Ease GrowToFitEase = Ease.Unset;

    [SerializeField]
    [Range( 4f, 10f )]
    private float speed = 10f;

    #endregion

    /* Life Cycle */

    private void Awake()
    {
        edgeTag_ = edgePrefab.tag;
        tileTags_ = new List<string>();
        foreach ( GameObject gameObject in tilePrefabs )
        {
            tileTags_.Add( gameObject.tag );
        }

        grid_ = new GameObject[ SIZE, SIZE ];

        CreateAt( topLeftEdgePrefab, 0, SIZE - 1 );
        CreateAt( topRightEdgePrefab, SIZE - 1, SIZE - 1 );
        CreateAt( bottomLeftEdgePrefab, 0, 0 );
        CreateAt( bottomRightEdgePrefab, SIZE - 1, 0 );

        for ( int x = 1; x < SIZE - 1; x++ )
        {
            CreateAt( bottomMiddleEdgePrefab, x, 0 );
            CreateAt( topMiddleEdgePrefab, x, SIZE - 1 );
        }

        for ( int y = 1; y < SIZE - 1; y++ )
        {
            CreateAt( middleLeftEdgePrefab, 0, y );
            CreateAt( middleRightEdgePrefab, SIZE - 1, y );
        }

        CreateAt( tilePrefabs[ 0 ], SIZE / 2 - 2, SIZE / 2 );
        CreateAt( tilePrefabs[ 1 ], SIZE / 2 - 1, SIZE / 2 );
        CreateAt( tilePrefabs[ 2 ], SIZE / 2, SIZE / 2 );
        CreateAt( tilePrefabs[ 3 ], SIZE / 2 + 1, SIZE / 2 );

        tiles_ = new List<GameObject>()
        {
            grid_[SIZE / 2 - 2, SIZE / 2],
            grid_[SIZE / 2 - 1, SIZE / 2],
            grid_[SIZE / 2, SIZE / 2],
            grid_[SIZE / 2 + 1, SIZE / 2],
        };
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.W ) )
        {
            move( Direction.UP );
        }
        else if ( Input.GetKeyDown( KeyCode.UpArrow ) )
        {
            move( Direction.UP );
        }
        else if ( Input.GetKeyDown( KeyCode.A ) )
        {
            move( Direction.LEFT );
        }
        else if ( Input.GetKeyDown( KeyCode.LeftArrow ) )
        {
            move( Direction.LEFT );
        }
        else if ( Input.GetKeyDown( KeyCode.S ) )
        {
            move( Direction.DOWN );
        }
        else if ( Input.GetKeyDown( KeyCode.DownArrow ) )
        {
            move( Direction.DOWN );
        }
        else if ( Input.GetKeyDown( KeyCode.D ) )
        {
            move( Direction.RIGHT );
        }
        else if ( Input.GetKeyDown( KeyCode.RightArrow ) )
        {
            move( Direction.RIGHT );
        }
        else if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            CycleActiveTile();
        }
        else if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            CycleActiveTile();
        }
        else if ( Input.GetKeyDown( KeyCode.Tab ) )
        {
            CycleActiveTile();
        }
        else if ( Input.GetKeyDown( KeyCode.R ) )
        {
            Restart();
        }
    }

    /* Public */

    public const int SIZE = 10;
    public const int VICTORY_SIZE = 2;

    public void SetActiveTile( GameObject gameObject )
    {
        activeTileIndex_ = tiles_.IndexOf( gameObject );
        Shake( gameObject );
    }

    /* Private */

    private string edgeTag_ = null;
    private List<string> tileTags_ = null;

    private List<GameObject> tiles_ = null;
    private int activeTileIndex_ = 0;

    private GameObject activeTile_ => tiles_[ activeTileIndex_ ];
    private Coordinate ActiveTileCoordinate
    {
        get
        {
            for ( int x = 0; x < SIZE; x++ )
            {
                for ( int y = 0; y < SIZE; y++ )
                {
                    if ( activeTile_.Equals( grid_[ x, y ] ) )
                    {
                        return new Coordinate() { x = x, y = y };
                    }
                }
            }

            return new Coordinate();
        }
    }

    private GameObject[,] grid_ = null;

    private Vector3 BottomLeftCorner => transform.localPosition - SIZE / 2 * Vector3.one;

    private bool isMoving = false;

    private void Restart()
    {
        SceneManager.LoadScene( SceneManager.GetActiveScene().name );
    }

    private void Shake( GameObject gameObject )
    {
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.DOShakeScale( 0.25f, 0.25f ).OnComplete( () => {
            gameObject.transform.localScale = Vector3.one;
        } );
    }

    private void CheckVictory()
    {
        if ( IsMended() )
        {
            GameObject container = new GameObject( "container" );

            Vector3 p = Vector3.zero;
            foreach ( GameObject tile in tiles_ )
            {
                p += tile.transform.localPosition;
            }
            p = p / tiles_.Count;
            container.transform.position = p;

            foreach ( GameObject tile in tiles_ )
            {
                tile.transform.SetParent( container.transform );
            }

            float distanceToMove = Vector2.Distance( Vector3.zero, container.transform.position );
            float time = distanceToMove / speed / 2;

            container.transform.DOScale( 0, 1 ).SetEase( GrowToFitEase ).OnComplete( () => {
                container.transform.position = Vector3.zero;
                container.transform.DOScale( 4, 2 ).SetEase( GrowToFitEase );
            } );
        }
    }

    private void CycleActiveTile()
    {
        SetActiveTile( tiles_[ ( activeTileIndex_ + 1 ) % tiles_.Count ] );
    }

    private void PlaceAt( GameObject gameObject, int x, int y )
    {
        PlaceAt( gameObject, new Coordinate() { x = x, y = y } );
    }

    private void PlaceAt( GameObject gameObject, Coordinate coordinate )
    {
        gameObject.transform.localPosition = BottomLeftCorner + Vector3.one / 2 + new Vector3( coordinate.x, coordinate.y, 0 );
        grid_[ coordinate.x, coordinate.y ] = gameObject;
    }

    private void CreateAt( GameObject prefab, int x, int y )
    {
        CreateAt( prefab, new Coordinate() { x = x, y = y } );
    }

    private void CreateAt( GameObject prefab, Coordinate coordinate )
    {
        GameObject instance = Instantiate( prefab, transform );
        PlaceAt( instance, coordinate );
        instance.name = coordinate.ToString();
    }

    private Coordinate destination( Direction direction )
    {
        Coordinate d = ActiveTileCoordinate;
        switch ( direction )
        {
            case Direction.UP:
                for ( int y = d.y + 1; y < SIZE; y++ )
                {
                    if ( grid_[ d.x, y ] != null )
                    {
                        break;
                    }

                    d.y = y;
                }

                break;

            case Direction.DOWN:
                for ( int y = d.y - 1; y > 0; y-- )
                {
                    if ( grid_[ d.x, y ] != null )
                    {
                        break;
                    }
                    d.y = y;
                }
                break;

            case Direction.RIGHT:
                for ( int x = d.x + 1; x < SIZE; x++ )
                {
                    if ( grid_[ x, d.y ] != null )
                    {
                        break;
                    }

                    d.x = x;
                }
                break;

            case Direction.LEFT:
                for ( int x = d.x - 1; x > 0; x-- )
                {
                    if ( grid_[ x, d.y ] != null )
                    {
                        break;
                    }
                    d.x = x;
                }
                break;
        }

        return d;
    }

    private void move( Direction direction )
    {
        if ( isMoving || IsMended() )
        {
            return;
        }

        isMoving = true;
        Coordinate oldCoordinate = ActiveTileCoordinate;
        Coordinate newCoordinate = destination( direction );
        if ( !oldCoordinate.Equals( newCoordinate ) )
        {
            Vector3 dest = BottomLeftCorner + Vector3.one / 2 + new Vector3( newCoordinate.x, newCoordinate.y, 0 );

            float distanceToMove = Vector2.Distance( dest, activeTile_.transform.position );
            float time = distanceToMove / speed;

            activeTile_.transform.DOMove( dest, time ).SetEase( MovementEase ).OnComplete( () => {
                isMoving = false;
                foreach ( GameObject tile in tiles_ )
                {
                    float distance = Vector2.Distance( tile.transform.localPosition, activeTile_.transform.localPosition );
                    if ( distance < 1.8f && distance > 0.2f )
                    {
                        Shake( tile );
                    }
                }

                activeTile_.transform.localScale = Vector3.one;
                activeTile_.transform.DOShakeScale( 0.2f, 0.2f ).OnComplete( () => {
                    activeTile_.transform.localScale = Vector3.one;
                    CheckVictory(); // race condition
                } );
            } );

            grid_[ newCoordinate.x, newCoordinate.y ] = activeTile_;
            grid_[ oldCoordinate.x, oldCoordinate.y ] = null;
        }
        else
        {
            isMoving = false;
        }
    }

    private bool IsMended()
    {
        for ( int bottomLeftX = 0; bottomLeftX < SIZE - VICTORY_SIZE; bottomLeftX++ )
        {
            for ( int bottomLeftY = 0; bottomLeftY < SIZE - VICTORY_SIZE; bottomLeftY++ )
            {
                int count = 0;

                for ( int x = bottomLeftX; x < SIZE && x < bottomLeftX + VICTORY_SIZE; x++ )
                {
                    for ( int y = bottomLeftY; y < SIZE && y < bottomLeftY + VICTORY_SIZE; y++ )
                    {
                        if ( grid_[ x, y ] != null && !grid_[ x, y ].CompareTag( edgeTag_ ) )
                        {
                            count++;
                        }
                    }
                }

                if ( count == VICTORY_SIZE * VICTORY_SIZE )
                {
                    return true;
                }
            }
        }

        return false;
    }
}
