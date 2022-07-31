using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using TMPro;

namespace MyProject
{
    public class OmokPlayer : MonoBehaviour
    {
        private OmokGame game;
        private sbyte myPlayerID;
        public PhotonView photonView;

        private float timer = 0f;

        [SerializeField] private GameObject boardPrefab = null;
        [SerializeField] private GameObject blackPiecePrefab = null;
        [SerializeField] private GameObject whitePiecePrefab = null;

        [SerializeField] private Transform board = null;
        [SerializeField] private Transform pieceParent = null;
        [SerializeField] private LineRenderer matchLine = null;
        [SerializeField] private TextMeshProUGUI tempText = null;

        private GameObject[,] pieces;

        private bool actionLock = false;

        private void Awake()
        {
            this.game = new OmokGame(7, 7, 5);
            this.pieces = new GameObject[this.game.board.Row, this.game.board.Column];
            for (int i = 0; i < this.pieces.GetLength(0); i++)
                for (int j = 0; j < this.pieces.GetLength(1); j++)
                    this.pieces[i, j] = null;

            for (int i = 0; i < this.game.board.Row; i++)
                for (int j = 0; j < this.game.board.Column; j++)
                    Instantiate(this.boardPrefab, BoardToWorld(i, j) + Vector3.forward * 1f, Quaternion.identity, this.board);

            this.game.onGameEnd += OnGameEnd;
        }

        private void Start()
        {
            this.myPlayerID = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? (sbyte)1 : (sbyte)-1;
        }

        private void Update()
        {
            tempText.text = $"Mine: {myPlayerID}\nCurrent turn: {this.game.player}";
            if (!this.actionLock)
            {
                if (Input.GetMouseButtonDown(0) && this.game.player == myPlayerID)
                {
                    Vector3 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (WorldToBoard(worldMouse, out int r, out int c))
                    {
                        this.actionLock = true;
                        photonView.RPC(nameof(DoAction), RpcTarget.All, r, c);
                    }
                }
            }
        }

        [PunRPC]
        private void DoAction(int r, int c)
        {
            if (this.game.TakeAction(r, c))
            {
                UpdatePieces();
            }

            this.actionLock = false;
        }

        private void AITurn()
        {
            int action = new Minimax(this.game.logic, 5).Find(this.game.board, false);
            this.game.TakeAction(action);
            UpdatePieces();
        }

        private Vector3 BoardToWorld(int r, int c)
        {
            return new Vector3((r - this.game.board.Row * 0.5f) * 1f, (c - this.game.board.Column * 0.5f) * 1f, 0f);
        }

        private bool WorldToBoard(Vector3 pos, out int r, out int c)
        {
            r = Mathf.RoundToInt(1f * pos.x + this.game.board.Row * 0.5f);
            c = Mathf.RoundToInt(1f * pos.y + this.game.board.Column * 0.5f);
            return r >= 0 && r < this.game.board.Row && c >= 0 && c < this.game.board.Column;
        }

        private void UpdatePieces()
        {
            for (int i = 0; i < this.pieces.GetLength(0); i++)
                for (int j = 0; j < this.pieces.GetLength(1); j++)
                {
                    sbyte p = this.game.board.Get(i, j);
                    if (this.pieces[i, j] == null)
                    {
                        if (p != 0)
                        {
                            this.pieces[i, j] = Instantiate(p == 1 ? this.blackPiecePrefab : this.whitePiecePrefab, BoardToWorld(i, j), Quaternion.identity, this.transform);
                        }
                    }
                    else
                    {
                        if (p == 0)
                        {
                            Destroy(this.pieces[i, j]);
                            this.pieces[i, j] = null;
                        }
                    }
                }
        }

        private void OnGameEnd(sbyte winner, RowCol p1, RowCol p2)
        {
            Debug.Log($"{winner} beat {-winner}");
            this.matchLine.enabled = true;
            this.matchLine.SetPosition(0, BoardToWorld(p1.r, p1.c) + Vector3.back * 2f);
            this.matchLine.SetPosition(1, BoardToWorld(p2.r, p2.c) + Vector3.back * 2f);
        }
    }
}