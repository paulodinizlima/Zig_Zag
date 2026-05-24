using UnityEngine;
using UnityEngine.EventSystems;

public class TapToStartButton : MonoBehaviour, IPointerClickHandler
{
	/*
	Este script permite iniciar o jogo clicando diretamente
	no texto/área "Tap to Start".

	Ele é útil porque textos com Raycast Target ligado
	bloqueiam o clique de fundo, então precisamos tratar
	o clique neles explicitamente.
	*/

	public void OnPointerClick(PointerEventData eventData)
	{
		if (GameplayController.instance == null) {
			//Debug.LogWarning("GameplayController.instance não encontrado.");
			return;
		}

		if (GameplayController.instance.gamePlaying) {
			return;
		}

		if (StartUI.instance != null) {
			StartUI.instance.HideStartPanel();
		}

		GameplayController.instance.StartGameplay();
	}
}