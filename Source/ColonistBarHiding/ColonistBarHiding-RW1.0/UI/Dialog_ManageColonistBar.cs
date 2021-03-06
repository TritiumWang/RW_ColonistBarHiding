﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace ColonistBarHiding.UI
{
	/// <summary>
	/// A UI dialog for managing the colonist bar, specifically for marking
	/// and unmarking which colonists are hidden.
	/// </summary>
	public class Dialog_ManageColonistBar : Window
	{
		private readonly bool fromColonistBar;

		private Vector2 scrollPosition;
		private float viewHeight;

		new const float Margin = 10f;

		public override void DoWindowContents(Rect inRect)
		{
			var rect = inRect.ContractedBy(Margin);
			// Avoid overlapping with close button
			const float distFromBottom = 40f;
			rect.height -= distFromBottom;
			var rect4 = new Rect(0, 0, rect.width - 16f, this.viewHeight);

			Widgets.BeginScrollView(rect, ref scrollPosition, rect4, true);
			var rect5 = rect4;
			rect5.width -= 16f;
			rect5.height = 9999f;

			Listing_Standard list = new Listing_Standard()
			{
				ColumnWidth = rect5.width,
				maxOneColumn = true,
				verticalSpacing = 6f
			};
			list.Begin(rect5);

			var pawns = ColonistBarUtility.GetColonistBarPawns();
			foreach (var pawn in pawns)
			{
				var pawnRect = list.GetRect(6f);
				if (pawn == null)
				{
					continue;
				}
				AddButton(pawnRect, list, pawn, fromColonistBar);
			}

			if (Event.current.type == EventType.Layout)
			{
				this.viewHeight = list.CurHeight;
			}
			list.End();
			Widgets.EndScrollView();
		}

		// TODO: Add pawn icon in menu, next to name
		private static void AddButton(Rect rect, Listing_Standard list, Pawn pawn, bool fromColonistBar)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}
			if (pawn == null)
			{
				throw new ArgumentNullException(nameof(pawn));
			}
			float truncWidth = rect.width / 2f - 4f;
			string pawnLabel = pawn.Label.Truncate(truncWidth);

			if (ColonistBarUtility.IsHidden(pawn))
			{
				string buttonLabel = "ColonistBarHiding.Restore".Translate();
				buttonLabel = buttonLabel.Truncate(truncWidth);
				if (list.ButtonTextLabeled(pawnLabel, buttonLabel))
				{
					ColonistBarUtility.RestoreColonist(pawn);
				}
			}
			else
			{
				string buttonLabel = "ColonistBarHiding.Remove".Translate();
				buttonLabel = buttonLabel.Truncate(truncWidth);
				if (list.ButtonTextLabeled(pawnLabel, buttonLabel))
				{
					ColonistBarUtility.RemoveColonist(pawn, fromColonistBar);
				}
			}
		}

		public Dialog_ManageColonistBar(bool fromColonistBar)
		{
			this.forcePause = true;
			this.doCloseX = true;
			this.doCloseButton = true;
			this.closeOnClickedOutside = true;
			this.absorbInputAroundWindow = true;
			this.fromColonistBar = fromColonistBar;
		}
	}
}
