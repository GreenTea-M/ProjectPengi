Note: most of the variables seen in the commands below are in either
gameConfiguration asset in the Assets/Data or in IconManager and CustomCommands
in Assets/Scene/DialogScene scene.

<table>
    <thead>
        <tr>
            <th colspan="3">playSfx</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;playSfx sfxName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Plays a sound effect once. Sound effects can stack on each other.
            <br/>
            The sound effect played is determined by the name in the audioList in the CustomCommands component.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;playSfx bearBell&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">enterStage</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;enterStage hank&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Allows a character to appear on the scene. You can use aliases for the parameter, and you can use one command for multiple characters.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;enterStage Byrnhilda&gt;&gt;
            &lt;&lt;enterStage B Hank oldMole&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">exitStage</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;playSfx sfxName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            This forces a character to leave the scene. After a character
            leaves a scene and if their name appears in the script as the speaker,
            they will not appear and the narrator will take place of them speaking.
            When this happens, a warning message will appear in Debug.Log about this.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;exitStage Byrnhilda&gt;&gt;
            &lt;&lt;exitStage B Hank oldMole&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">gameEnd</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;gameEnd&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Calling GameEnd will make the screen fade to black and open the credit scene.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;gameEnd&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">hideDialogue</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;hideDialogue&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Forces all characters to leave without the side effect of them not reappearing when
            they need to speak.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;hideDialogue&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Note</td>
            <td colspan="2">
            The game had more distinct elements back then. Now, it's just a matter of making
            the characters leave the scene without the side effect of not making them not appear
            when it's their turn to speak.
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">showDialogue</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;showDialogue&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Does nothing.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;showDialogue&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Note</td>
            <td colspan="2">
            The game had more distinct elements back then. Now, it's just a matter of making
            the characters leave the scene without the side effect of not making them not appear
            when it's their turn to speak.<br>
            Since hideDialogue no longer has the side effect of making characters
            leave permanently, showDialogue has no more purpose.
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">showItem</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;showItem itemName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Spawns a showable item into the scene. itemName should match
            the name of the showableItemData in showableItemDataList in
            the CustomCommands component.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;showItem fadingMole&gt;&gt;
            &lt;&lt;showItem shelf&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">hideItem</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;hideItem itemName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Destroys or informs a showable item that's already in the scene that they need to be destroyed. itemName should match
            the name of the showableItemData in showableItemDataList in
            the CustomCommands component.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;hideItem fadingMole&gt;&gt;
            &lt;&lt;hideItem shelf&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">resetSpeaker</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;resetSpeaker&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Does not do anything.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;resetSpeaker&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Note</td>
            <td colspan="2">
            This used to reset the state of the characters in the scene.
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">changeHeader</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;changeHeader headerName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Changes the current active header.
            It also animates the location plate if the display name is not empty.
            The data for each header are in the CustomCommands component.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;changeHeader cockpit&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">changeBackground</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;changeBackground headerName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Changes the current active header.
            It also animates the location plate if the display name is not empty.
            The data for each header are in the CustomCommands component.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;changeBackground cockpit&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Note</td>
            <td colspan="2">
            This is an alias for changeHeader
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">debugLog</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;debugLog your message here&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Prints a message into Debug.LogWarning()
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;debugLog invalid state I don't know what is happening&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">playAudio</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;playAudio audioName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Plays the looping backgroudn music. In the game, only one background music can be played at a time.
            When invoked, it will attempt to fade out the previous audio, and fade in the current audio requested.
            The names here are the same source as playSfx. Check out the audioList in the CustomCommands component.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;playAudio oldMole&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">clearShelfItem</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;clearShelfItem&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Makes all shelf item disappear. These shelf items are from the interactive section in Byrnhilda's part.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;clearShelfItem&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">doPuzzle</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;doPuzzle puzzleName&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            The only suppported puzzleName is "shelf". "shelf" will activate
            the interactive section in Byrnhilda's part.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;doPuzzle shelf&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Note</td>
            <td colspan="2">
            Originally, there were supposed to be puzzles. The puzzles would fall apart,
            and you would have to put them in their proper positions. Then, we just
            scrapped the idea. The current mechanic is a lot more simplified than
            what was originally planned.
            </td>
        </tr>
    </tbody>
</table>

<table>
    <thead>
        <tr>
            <th colspan="3">fakeLastDialog</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;fakeLastDialog your message here&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Sets the last dialog to the given parameter without making the dialog
            appear on-screen. This is very useful for parts without dialog, and we
            still want to give context about the last dialog when saving. For example,
            a node without a dialog would be during Byrnhilda's story where you
            pick up the items.
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;fakeLastDialog I just finished cleaning up the shelf...&gt;&gt;
            </td>
        </tr>
    </tbody>
</table>


<table>
    <thead>
        <tr>
            <th colspan="3">fadePlainBackground</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Syntax</td>
            <td colspan="2">
            &lt;&lt;fadePlainBackground onOff duration block? color&gt;&gt;
            &lt;&lt;fadePlainBackground onOff duration block? r g b a&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Description</td>
            <td colspan="2">
            Fades in or fades out a plain foreground
            Arguments:
            <ol>
            <li>
            onOff
            <ul>
            <li>on: will make the foreground appear</li>
            <li>off: will make the foreground disappear</li>
            </ul>
            </li>
            <li>duration: float as transition duration</li>
            <li>if block? equals to "block","the function will block yarn until it finishes its transition</li>
            <li>
            Color would be a color name in English to fade into.
            The only supported colors right now are white and black.
            It has no effect for fading out.
            </li>
            <li>
            r, g, b, a would be the values in Color to fade into.
            It has no effect for fading out.
            </li>
            </ol>
            </td>
        </tr>
        <tr>
            <td>Example</td>
            <td colspan="2">
            &lt;&lt;fadePlainBackground on 0.5 block white&gt;&gt;
            &lt;&lt;fadePlainBackground off 1.2 noblock 0.5 0.5 0.5 0.5&gt;&gt;
            </td>
        </tr>
        <tr>
            <td>Note</td>
            <td colspan="2">
            This was poorly named.
            </td>
        </tr>
    </tbody>
</table>