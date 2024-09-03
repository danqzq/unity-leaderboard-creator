var createCopyable = {
    CopyToClipboard: function(arg) 
    {
        window.becauseUnitysBadWithJavacript_createCopyableInBrowser = 
             window.becauseUnitysBadWithJavacript_createCopyableInBrowser || 
             {
                 busy: false,
                 initialized: false,
                 rootDisplayStyle: null,  // style to make root element visible
                 root_: null,             // root element of form
                 ctx_: null,              // canvas for getting image data;
             };
        var g = window.becauseUnitysBadWithJavacript_createCopyableInBrowser;
        if (g.busy) 
        {
            // Don't let multiple requests come in
            return;
        }
        g.busy = true;

        var textToCopy = UTF8ToString(arg);
       
        if (!g.initialized) 
        {
            g.initialized = true;
            g.ctx = window.document.createElement("canvas").getContext("2d");
       
            // Append a form to the page (more self contained than editing the HTML?)
            g.root = window.document.createElement("div");
            g.root.innerHTML = [
              '<style>                                                  ',
              '.gettext {                                               ',
              '    position: absolute;                                  ',
              '    left: 0;                                             ',
              '    top: 0;                                              ',
              '    width: 100%;                                         ',
              '    height: 100%;                                        ',
              '    display: -webkit-flex;                               ',
              '    display: flex;                                       ',
              '    -webkit-flex-flow: column;                           ',
              '    flex-flow: column;                                   ',
              '    -webkit-justify-content: center;                     ',
              '    -webkit-align-content: center;                       ',
              '    -webkit-align-items: center;                         ',
              '                                                         ',
              '    justify-content: center;                             ',
              '    align-content: center;                               ',
              '    align-items: center;                                 ',
              '                                                         ',
              '    z-index: 2;                                          ',
              '    color: white;                                        ',
              '    background-color: rgba(0,0,0,0.75);                  ',
              '    font: sans-serif;                                    ',
              '    font-size: x-large;                                  ',
              '}                                                        ',
              '.gettext a,                                              ',
              '.gettext p {                                             ',
              '   font-size: x-large;                                   ',
              '   background-color: #666;                               ',
              '   border-radius: 0.5em;                                 ',
              '   border: 1px solid black;                              ',
              '   padding: 0.5em;                                       ',
              '   margin: 0.25em;                                       ',
              '   outline: none;                                        ',
              '   display: inline-block;                                ',
              '}                                                        ',
              '                                                         ',
              '.gettext a:hover {cursor: pointer;}                      ',
              '                                                         ',
              '</style>                                                 ',
              '<div class="gettext">                                    ',
              '  <div>                                                  ',
              '    <p class="copyable">' + textToCopy + '</p>           ',
              '    <a>Done</a>                                          ',
              '  </div>                                                 ',
              '</div>                                                   ',
            ].join('\n');
            
            var label = g.root.querySelector(".copyable");
            label.addEventListener('click', preventOtherClicks);
       
            // clicking cancel or outside cancels
            var cancel = g.root.querySelector("a");  // there's only one
            cancel.addEventListener('click', handleCancel);
       
            // remember the original style
            g.rootDisplayStyle = g.root.style.display;
       
            window.document.body.appendChild(g.root);
        }
	else {
	    var copyable = g.root.querySelector(".copyable");
	    copyable.innerHTML = textToCopy;
	}
       
        // make it visible
        g.root.style.display = g.rootDisplayStyle;
       
        function preventOtherClicks(evt) 
        {
            evt.stopPropagation();
        }
        
        function handleCancel(evt) 
        {
            evt.stopPropagation();
            evt.preventDefault();
            hide();
            g.busy = false;
        }
        
        function hide() 
        {
            g.root.style.display = "none";
        }
    },
};     
       
mergeInto(LibraryManager.library, createCopyable);
       
       
       