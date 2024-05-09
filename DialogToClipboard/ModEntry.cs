using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Quests;

namespace YourProjectName
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }


        /*********
        ** Private methods
        *********/

        List<Type> excludedMenus = new List<Type>() {typeof(ItemGrabMenu)};
        IClickableMenu? currentMenu = null;
        bool inMenu = false;
        String GrabbedText = "";
        /// <summary>Raised when player changes menu.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if(!Context.IsWorldReady)
            {
                return;
            }
            if(e.NewMenu != null && e.OldMenu == null)
            {        
                currentMenu = e.NewMenu;
                inMenu = true;
                return;        
            }
            else if(e.NewMenu == null && e.OldMenu != null)
            {
                inMenu = false;
                GrabbedText = "";
            }
        }
        /// <summary>Raised at each Tick of the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if(!inMenu || GrabbedText != "" || currentMenu == null )
            {
                return;
            }
            Type menuType =  currentMenu.GetType();
            if(excludedMenus.Contains(menuType))
            {
                return;            
            }
            else if(menuType.Equals(typeof(DialogueBox)))
            {
                DialogueBox curMenu = (DialogueBox)currentMenu;
                GrabbedText = curMenu.getCurrentString();
            }
            else if(menuType.Equals(typeof(QuestLog)))
            {
                QuestLog qlog = (QuestLog)currentMenu;
                IReflectedField<IQuest> currentQuest = this.Helper.Reflection.GetField<IQuest>(qlog,"_shownQuest");
                if(currentQuest != null && currentQuest.GetValue() != null)
                {
                    GrabbedText = currentQuest.GetValue().GetName() + ":" + currentQuest.GetValue().GetDescription();
                }
                else
                {
                    return;
                }
            }
            else
            {
                this.Monitor.Log(menuType.ToString(), LogLevel.Debug);
                return;
            }
            this.print(processText(GrabbedText));
        }        
        
        /// <summary>Used to strip special characters from menu text.</summary>
        /// <param name="text">The input text to be processed</param>
        private String processText(string text)
        {
            String newText = text.Replace("^", "\n");
            return newText;
        }
        private void print(string text)
        {
            StardewValley.DesktopClipboard.SetText(text);
            this.Monitor.Log(text, LogLevel.Debug);
        }
    }
}