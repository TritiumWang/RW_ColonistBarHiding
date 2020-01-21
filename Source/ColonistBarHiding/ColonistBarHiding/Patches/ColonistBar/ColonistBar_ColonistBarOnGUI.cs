﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;
using RimWorld;
using Verse;

namespace ColonistBarHiding.Patches.ColonistBar
{
	using ColonistBar = RimWorld.ColonistBar;

	/// <summary>
	/// Patch for ColonistBar.ColonistBarOnGUI(), which handles the colonist
	/// bar's display on each OnGUI() call. This patch accounts for hidden
	/// colonists.
	/// </summary>
	[HarmonyPatch(typeof(ColonistBar))]
	[HarmonyPatch("ColonistBarOnGUI")]
	internal class ColonistBar_ColonistBarOnGUI
	{
		private static void HandleGroupFrameClicks(int currentGroup)
		{
			if (ColonistBarUtility.ShowGroupFrames)
			{
				var colonistBar = Find.ColonistBar;
				var entries = colonistBar.Entries;
				// Possible bug if entries and draw locs do not line up
				//if (entries.Count != colonistBar.DrawLocs.Count)
				//{
				//	throw new InvalidOperationException(
				//		$"Entries count ({entries.Count}) must be equal to draw locs count ({colonistBar.DrawLocs.Count}).");
				//}
				for (int i = 0; i < colonistBar.DrawLocs.Count; i++)
				{
					var entry = entries[i];
					bool flag = currentGroup != entry.group;
					currentGroup = entry.group;
					if (flag)
					{
						colonistBar.drawer.HandleGroupFrameClicks(entry.group);
					}
				}
			}
		}

		private static void HandleRepaint(ColonistBar.Entry entry, Rect rect, bool isDifferentGroup, bool reordering, List<Pawn> colonistsToHighlight)
		{
			if (Event.current.type == EventType.Repaint)
			{
				if (isDifferentGroup && ColonistBarUtility.ShowGroupFrames)
				{
					Find.ColonistBar.drawer.DrawGroupFrame(entry.group);
				}
				if (entry.pawn != null)
				{
					Find.ColonistBar.drawer.DrawColonist(rect, entry.pawn, entry.map, colonistsToHighlight.Contains(entry.pawn), reordering);
				}
			}
		}

		private static int GetReorderableGroup(ColonistBar colonistBar, ColonistBar.Entry entry, List<ColonistBar.Entry> cachedEntries)
		{
			return ReorderableWidget.NewGroup(delegate (int from, int to)
			{
				ColonistBarUtility.Reorder(from, to, entry.group, cachedEntries);
			}, ReorderableDirection.Horizontal, colonistBar.SpaceBetweenColonistsHorizontal, delegate (int index, Vector2 dragStartPos)
			{
				ColonistBarUtility.DrawColonistMouseAttachment(index, dragStartPos, entry.group, cachedEntries);
			});
		}

		// Modified private method ColonistBar.ColonstBarOnGUI()
		private static void ColonistBarOnGUI(List<ColonistBar.Entry> cachedEntries, List<Pawn> colonistsToHighlight)
		{
			if (!ColonistBarUtility.ShouldBeVisible())
			{
				return;
			}
			if (Event.current.type != EventType.Layout)
			{
				var colonistBar = Find.ColonistBar;
				var entries = ColonistBarUtility.GetVisibleEntries();
				int currentGroup = -1;
				bool showGroupFrames = ColonistBarUtility.ShowGroupFrames;
				int reorderableGroup = -1;
				for (int i = 0; i < colonistBar.DrawLocs.Count; i++)
				{
					var rect = ColonistBarUtility.GetRect(i);
					var entry = entries[i];

					bool isDifferentGroup = currentGroup != entry.group;
					currentGroup = entry.group;
					if (isDifferentGroup)
					{
						reorderableGroup = GetReorderableGroup(colonistBar, entry, entries);
					}
					bool reordering;
					if (entry.pawn != null)
					{
						colonistBar.drawer.HandleClicks(rect, entry.pawn, reorderableGroup, out reordering);
					}
					else
					{
						reordering = false;
					}
					HandleRepaint(entry, rect, isDifferentGroup, reordering, colonistsToHighlight);
				}
				currentGroup = -1;
				HandleGroupFrameClicks(currentGroup);
			}
			if (Event.current.type == EventType.Repaint)
			{
				colonistsToHighlight.Clear();
			}
		}

		[HarmonyPrefix]
		private static bool Prefix(List<ColonistBar.Entry> ___cachedEntries, List<Pawn> ___colonistsToHighlight)
		{
			ColonistBarOnGUI(___cachedEntries, ___colonistsToHighlight);
			return false;
		}
	}
}
