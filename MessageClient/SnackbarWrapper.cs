using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MessageClient;
using System;

namespace MessageClinet
{
    public class SnackbarWrapper
    {
        private readonly string text;
        private readonly int duration;
        private readonly IWindowManager windowManager;
        private readonly Context appplicationContext;
        private Snackbar.Callback externalCallback;
        private SnackbarAction action { get; set; }

        public static SnackbarWrapper make(Context applicationContext, string text, int duration)
        {
            return new SnackbarWrapper(applicationContext, text, duration);
        }

        private SnackbarWrapper(Context appplicationContext, string text, int duration)
        {
            this.appplicationContext = appplicationContext;
            var wm = appplicationContext.GetSystemService(Context.WindowService);
            // We have to use JavaCast instead of a normal cast
            this.windowManager = wm.JavaCast<IWindowManager>();
            this.text = text;
            this.duration = duration;
        }

        public void Show()
        {
            WindowManagerLayoutParams layoutParams = createDefaultLayoutParams(WindowManagerTypes.Toast, null);
            var frameLayout = new FrameLayout(appplicationContext);
            frameLayout.ViewAttachedToWindow += delegate
            {
            //this.onAttachedToWindow();
            onRootViewAvailable(frameLayout);
            };

            windowManager.AddView(frameLayout, layoutParams);
        }

        private void onRootViewAvailable(FrameLayout rootView)
        {
            var ctw = new ContextThemeWrapper(appplicationContext, Resource.Style.Base_Theme_AppCompat);
            CoordinatorLayout snackbarContainer = new CoordinatorLayout(ctw);
            snackbarContainer.ViewAttachedToWindow += delegate
            {
                onSnackbarContainerAttached(rootView, snackbarContainer);
            };

            windowManager.AddView(snackbarContainer, createDefaultLayoutParams(WindowManagerTypes.ApplicationPanel, rootView.WindowToken));
        }

        private void onSnackbarContainerAttached(View rootView, CoordinatorLayout snackbarContainer)
        {
            Snackbar snackbar = Snackbar.Make(snackbarContainer, text, duration);

            snackbar.SetCallback(new SnackbarCallbackImpl(rootView, snackbarContainer, windowManager));

            if (action != null)
            {
                snackbar.SetAction(action.Text, action.Listener);
            }
            snackbar.Show();
        }

        private WindowManagerLayoutParams createDefaultLayoutParams(WindowManagerTypes type, IBinder windowToken)
        {
            WindowManagerLayoutParams layoutParams = new WindowManagerLayoutParams();
            layoutParams.Format = Format.Translucent;
            layoutParams.Width = ViewGroup.LayoutParams.MatchParent;
            /* Si ponemos aqui WrapContent en alguna ocasion en la que haya un action largo y el texto tambien, el snackbar puede volverse como loco
             * asi que usamos MatchParent. Aun asi sucede que a veces se puede mostrar en una linea o en dos el mismo texto, pero al menos no hace el temblor loco que de la otra forma*/
            layoutParams.Height = ViewGroup.LayoutParams.MatchParent;
            layoutParams.Gravity = GravityFlags.CenterHorizontal | GravityFlags.Bottom;
            layoutParams.Flags = WindowManagerFlags.NotTouchModal;
            layoutParams.Type = type;
            layoutParams.Token = windowToken;
            return layoutParams;
        }

        public SnackbarWrapper SetCallback(Snackbar.Callback callback)
        {
            this.externalCallback = callback;
            return this;
        }

        public SnackbarWrapper SetAction(string text, Action<View> listener)
        {
            action = new SnackbarAction(text, listener);
            return this;
        }

    }//class

    internal class SnackbarAction
    {
        public string Text { get; set; }
        public Action<View> Listener { get; set; }

        public SnackbarAction(string text, Action<View> listener)
        {
            Text = text;
            Listener = listener;
        }
    }

    internal class SnackbarCallbackImpl : Snackbar.Callback
    {
        public Snackbar.Callback externalCallback { get; set; }

        View rootView;
        CoordinatorLayout snackbarContainer;
        IWindowManager windowManager;

        public SnackbarCallbackImpl(View rootView, CoordinatorLayout snackbarContainer, IWindowManager windowManager)
        {
            this.rootView = rootView;
            this.snackbarContainer = snackbarContainer;
            this.windowManager = windowManager;
        }

        public override void OnShown(Snackbar snackbar)
        {
            base.OnShown(snackbar);
            externalCallback?.OnShown(snackbar);
        }

        public override void OnDismissed(Snackbar snackbar, int evt)
        {
            base.OnDismissed(snackbar, evt);

            // Clean up (NOTE! This callback can be called multiple times)
            if (snackbarContainer.Parent != null && rootView.Parent != null)
            {
                windowManager.RemoveView(snackbarContainer);
                windowManager.RemoveView(rootView);
            }

            externalCallback?.OnDismissed(snackbar, evt);
        }
    }
}
