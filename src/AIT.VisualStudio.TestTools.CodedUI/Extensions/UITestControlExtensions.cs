﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;

using AIT.VisualStudio.TestTools.CodedUI.Attributes;

using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace AIT.VisualStudio.TestTools.CodedUI.Extensions
{
    using ControlType = Microsoft.VisualStudio.TestTools.UITesting.ControlType;

    /// <summary>
    /// Contains extension methods for the <see cref="UITestControl"/> class.
    /// </summary>
    public static class UITestControlExtensions
    {
        /// <summary>
        /// Gets the specified control.
        /// </summary>
        /// <param name="container">The container.</param>
        public static T Find<T>(this UITestControl container) where T : UITestControl, new()
        {
            var automationIdAttribute = typeof(T).GetCustomAttributes(typeof(AutomationIdAttribute), true).OfType<AutomationIdAttribute>().FirstOrDefault();

            if (automationIdAttribute == null)
            {
                throw new ArgumentOutOfRangeException("AutomationIdAttribute not set on type " + typeof(T));
            }

            return Find<T>(container, automationIdAttribute.Id);
        }

        /// <summary>
        /// Gets the specified control.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="automationId">The automation identifier.</param>
        public static T Find<T>(this UITestControl container, string automationId) where T : UITestControl, new()
        {
            var propertyExpressionCollection = new PropertyExpressionCollection { { WpfControl.PropertyNames.AutomationId, automationId }, };

            return Find<T>(container, propertyExpressionCollection);
        }

        /// <summary>
        /// Gets the specified control.
        /// </summary>
        public static T Find<T>(this UITestControl container, PropertyExpressionCollection searchProperties) where T : UITestControl, new()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            var testControl = new T { Container = container };

            testControl.SearchProperties.AddRange(searchProperties);

            return testControl;
        }

        /// <summary>
        /// Checks whether the specified control exists.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">the control object was null.</exception>
        public static bool Exists(this UITestControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            try
            {
                return control.FindMatchingControls().Any();
            }
            catch (UITestControlNotAvailableException)
            {
                return false;
            }
            catch (ElementNotAvailableException)
            {
                return false;
            }
        }

        /// <summary>
        /// Clicks the specified control.
        /// </summary>
        public static void Click(this UITestControl control)
        {
            if (control == null) throw new ArgumentNullException("control");
            Mouse.Click(control);

            var name = control.Name;
            var wpfControl = control as WpfControl;

            if (wpfControl != null)
            {
                name = wpfControl.AutomationId;
            }

            Trace.WriteLine(control.GetType().Name + " " + name + " clicked.");
        }

        /// <summary>
        /// Determines whether the specified control is visible.
        /// </summary>
        private static bool IsVisible(this UITestControl testControl)
        {
            if (testControl == null)
            {
                throw new ArgumentNullException("testControl");
            }

            return !testControl.State.HasFlag(ControlStates.Invisible) && 
                   !testControl.State.HasFlag(ControlStates.Collapsed) && 
                   !testControl.State.HasFlag(ControlStates.Offscreen);
        }

        /// <summary>
        /// Clicks the on first visible child.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <exception cref="ArgumentNullException">the control object was null.</exception>
        public static void ClickOnFirstVisibleChild(this UITestControl control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            var firstVisibleControl = control.GetChildren(true).First(c => (c.ControlType == ControlType.Text) && c.IsVisible());

            firstVisibleControl.Click();
        }

        /// <summary>
        /// Gets the children including their children.
        /// </summary>
        /// <remarks>
        /// Calling this method can be very time consuming.
        /// </remarks>
        private static IEnumerable<UITestControl> GetChildren(this UITestControl control, bool includingChildren)
        {
            foreach (var child in control.GetChildren())
            {
                yield return child;

                if (includingChildren)
                {
                    foreach (var uiTestControls in child.GetChildren(true))
                    {
                        yield return uiTestControls;
                    }
                }
            }
        }
    }
}