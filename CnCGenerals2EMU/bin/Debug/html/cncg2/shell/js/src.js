function getOptions() {
    const request = {
        url: "/options/graphics/get"
    }
    shellaccesslayer.execute(request);
}

function FrontEndFullscreenTrue() {
    const request = {
        _resource: "/usersettings/apply",
        shellfullscreen: true,
    };
    shellaccesslayer.execute(request);
}

function FrontEndFullscreenFalse() {
    const request = {
        _resource: "/usersettings/apply",
        shellfullscreen: false,
    };
    shellaccesslayer.execute(request);
}

function GameFullscreen() {
    const request = {
        _resource: "/usersettings/apply",
        gamefullscreen: true,
    };
    shellaccesslayer.execute(request);
}

function SetFullscreenDimensions() {
    const request = {
        _resource: "/usersettings/apply",
        fullscreenwidth: 1920,
        fullscreenheight: 1080,
    };
    shellaccesslayer.execute(request);
}

function SetWindowedDimensions() {
    const request = {
        _resource: "/usersettings/apply",
        windowedwidth: 1920,
        windowedheight: 1080,
    };
    shellaccesslayer.execute(request);
}

function updateVolume(value) {
    const maxDisplayedValue = 10;
    const displayedValue = (value / 100) * maxDisplayedValue;

    // Map displayedValue to the full range (0% to 100%)
    const fullRangeValue = (displayedValue / maxDisplayedValue) * 100;

    document.getElementById('volumeValue').textContent = fullRangeValue.toFixed(0) + '%';
    MasterVolume(displayedValue);
}

function MasterVolume(displayedValue) {
    const maxSliderValue = 100;
    const actualValue = (displayedValue / maxSliderValue) * 100;

    const request = {
        _resource: "/usersettings/apply",
        mastervolume: actualValue,
    };

    shellaccesslayer.execute(request);
}

// Settings not yet tested
// Payload may be incorrect
// function EdgePan() {
//     const request = {
//         _resource: "/usersettings/apply",
//         edgepan: true,
//     };
//     shellaccesslayer.execute(request);
// }

// function EdgeScrollSpeed() {
//     const request = {
//         _resource: "/usersettings/apply",
//         edgescrollspeed: true,
//     };
//     shellaccesslayer.execute(request);
// }

// function MiddleMouseCameraDrag() {
//     const request = {
//         _resource: "/usersettings/apply",
//         middlemousecameradrag: true,
//     };
//     shellaccesslayer.execute(request);
// }

// function MoveModeAttack() {
//     const request = {
//         _resource: "/usersettings/apply",
//         movemodeattack: true,
//     };
//     shellaccesslayer.execute(request);
// }

// function AllowDeselect() {
//     const request = {
//         _resource: "/usersettings/apply",
//         allowdeselect: true,
//     };
//     shellaccesslayer.execute(request);
// }

function applyAudio() {
    const request = {
        _resource: "/usersettings/applyAudio",
    };
    shellaccesslayer.execute(request);
}

function save() {
    const request = {
        _resource: "/usersettings/save",
    };
    shellaccesslayer.execute(request);
}

function discard() {
    const request = {
        _resource: "/usersettings/discard",
    };
    shellaccesslayer.execute(request);
}

// Execute an expression and evaluate
function executeExpression() {
    // Get the input value
    var expression = document.getElementById('expressionInput').value;

    try {
        // Evaluate the expression
        var result = eval(expression);

        // Display the result
        document.getElementById('resultBox').innerText = 'Result: ' + result;
    } catch (error) {
        // Handle any errors
        document.getElementById('resultBox').innerText = 'Error: ' + error.message;
    }
}
// Show objects from a given evaluated expression
function ShowObjects() {
    var command = document.getElementById('expressionInput').value;
    var resultBox = document.getElementById('resultBox');
    
    try {
        var objs = Object.getOwnPropertyNames(eval(command));
        resultBox.innerText = 'Objects: ' + objs.join(', ');
    } catch (error) {
        resultBox.innerText = 'Error: ' + error.message;
    }
}

function refreshWindow() {
    // Reload the current window
    location.reload();
}

function quitClient() {
    gameclient.execute('RTSClient.Quit');
}

var drawScreenInfoEnabled = false;

function toggleDrawScreenInfo() {
    drawScreenInfoEnabled = !drawScreenInfoEnabled;
    drawScreenInfo(drawScreenInfoEnabled);
}

function drawScreenInfo(enable) {
    if (enable) {
        gameclient.execute('Render.DrawInfo true');
        gameclient.execute('Render.DrawFpsHistogram true');
        gameclient.execute('Render.DrawScreenInfo true');
    } else {
        gameclient.execute('Render.DrawInfo false');
        gameclient.execute('Render.DrawFpsHistogram false');
        gameclient.execute('Render.DrawScreenInfo false');            
    }
}

var fpsIncreaseEnabled = false;

function toggleFpsIncrease() {
    fpsIncreaseEnabled = !fpsIncreaseEnabled;
    fpsIncrease(fpsIncreaseEnabled);
}

function fpsIncrease(enable) {
    if (enable) {
        gameclient.execute('GameTime.MaxSimFps 60');
        gameclient.execute('GameTime.MaxVariableFps 60');
        gameclient.execute('GameTime.MaxInactiveVariableFps 60');
    } else {
        gameclient.execute('GameTime.MaxSimFps 30');
        gameclient.execute('GameTime.MaxVariableFps 30');
        gameclient.execute('GameTime.MaxInactiveVariableFps 30');            
    }
}

function createGame() {
    var configFile = 'C:\\CNCO\\level.cfg';
    var levelName = 'Levels/SP/Alpha_Tutorial/Alpha_Tutorial';
    var playerId = 1;

    var command = 'RtsClient.createGame ' + configFile + ' ' + levelName + ' ' + playerId;
    console.log('Executing command:', command);

    try {
        gameclient.execute(command);
    } catch (error) {
        console.error('Error executing command:', error);
    }
}

// function createBlazeGame() {
//     var gameName = 'Test';
//     var numPlayers = 1;

//     // var result = shellaccesslayer.execute({_response: ShellResult, url: "/blaze/createGame?gameName=XEVRAC&players=4"});
//     var command = 'RtsClient.blazeCreateGame ' + gameName + ' ' + numPlayers;
//     console.log('Executing command:', command);

//     try {
//         gameclient.execute(command);
//     } catch (error) {
//         console.error('Error executing command:', error);
//     }
// }

function createBlazeGame() {
    shellaccesslayer.execute({
        _response: ShellResult,
        url: "/blaze/createGame?gameName=Player1&players=4"
    });
}

/// Authenticate v2

function blazeAuthenticate() {
    const request = {
        _resource: "/blaze/authenticate",
        email: "test@test.com",
        password: "test",
    };

    shellaccesslayer.execute({
        _response: ShellResult,
        url: "/blaze/authenticate?email=" + request.email + "&password=" + request.password
    });
}

function tokenAuthenticate() {
    shellaccesslayer.execute({
        _response: ShellResult,
        url: "/blaze/tokenauthenticate"
    });
}

function ShellResult(res) {
    // Handle the response here
    const debugOutput = document.getElementById("debugOutput");
    debugOutput.innerText = 'RESPONSE: ' + JSON.stringify(res, null, 2);

    // You can add additional logic based on the response if needed
    if (res.success) {
        console.log("Authentication successful!");
    } else {
        console.error("Authentication failed. Error: ", res.error);
    }
}

function blazeJoinGame() {
    const request = {
        url: "/blaze/joinGame?gameID=1"
    }
    shellaccesslayer.execute(request);
}


function blazeJoinGame1() {
    var hosting = "localhost",
    var playerId = 1;

    var command = 'RtsClient.joinGame ' + playerId + ' ' + hosting + ' ';
    console.log('Executing command:', command);

    try {
        gameclient.execute(command);
    } catch (error) {
        console.error('Error executing command:', error);
    }
}





function getConfig() {
    const request = {
        url: "/config/"
    }
    shellaccesslayer.execute(request);
}

function quitSession() {
    const request = {
        _resource: "/session/quit",
    };
    shellaccesslayer.execute(request);
}

function surrenderSession() {
    const request = {
        _resource: "/session/surrender",
    };
    shellaccesslayer.execute(request);
}