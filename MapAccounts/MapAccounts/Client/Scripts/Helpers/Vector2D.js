function Vector2D()
{
    var x;
    var y;

    this.X = function() { return x;  }
    this.Y = function() { return y;  }
    this.initialize = function(_x, _y)
    {
        x = _x;
        y = _y;
    }
}

Vector2D.add = function(a, b) { var v = new Vector2D(); v.initialize(a.X+b.X, a.Y+b.Y); return v;}
Vector2D.sub = function(a, b) { var v = new Vector2D(); v.initialize(a.X-b.X, a.Y-b.Y); return v;}
Vector2D.add = function(a, c) { var v = new Vector2D(); v.initialize(a.X*c, a.Y*c); return v;}
Vector2D.dot = function(a, b) {  return (a.X*b.X) + (a.Y*b.Y); }
Vector2D.norm = function(a) {  return Math.sqrt((a.x * a.x) + (a.y * a.y)); }