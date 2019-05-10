# Sub Model - nested clocks and counters

The second basic sample is a straight lift of the original Elmish.WPF sample with new view methods. In particular, this uses the same timer subscription model that the Elmish.WPF sample used, with no changes.

As noted in the single-counter readme, Xelmish doesn't provide much interface help. In particular, it requires every element given to have a specified x, y position, and usually a width, height size as well. It doesn't have any built-in layout controls or similar (though the base viewable functions are written in a way that might make creating one through partial application possible). Accordingly, with the nested components in this project, the sub component view functions are provided with a 'relative position' parameter, which is then passed in by the parent components. This provides a crude form of layout, and is worth noting.

The font used is 'SourceCodePro-Semibold' which is under the SIL Open Font License.
